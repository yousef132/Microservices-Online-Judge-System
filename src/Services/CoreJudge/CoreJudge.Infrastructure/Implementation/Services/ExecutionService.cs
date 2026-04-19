using CodeSphere.Domain.Abstractions.Services;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Text.RegularExpressions;

public class ExecutionService : IExecutionService
{
    private readonly DockerClient _dockerClient;
    private readonly IFileService fileService;
    private readonly string _requestId;
    private readonly string _requestDirectory;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private string _containerId = null;

    public ExecutionService(IFileService fileService, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _configuration = configuration;
        string dockerUriString = _configuration["DOCKER_HOST_URI"] ?? "unix:///var/run/docker.sock";
        var dockerUri = new Uri(dockerUriString);
        _dockerClient = new DockerClientConfiguration(dockerUri).CreateClient();

        _requestId = Guid.NewGuid().ToString();
        bool isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        string basePath = isRunningInContainer ? "/workspace" : (_configuration["HOST_EXECUTION_DIR"] ?? "/workspace");
        _requestDirectory = Path.Combine(basePath, "requests", _requestId);
        Directory.CreateDirectory(_requestDirectory);

        this.fileService = fileService;
    }

    public async Task<object> ExecuteCodeAsync(
        string code,
        ProblemLangeuageTemplates template,
        Language language,
        List<Testcase> testCases,
        decimal runTimeLimit)
    {
        string scriptPath = Helper.ScriptFilePath;
        if (File.Exists(scriptPath)) 
        {
            var content = await File.ReadAllTextAsync(scriptPath);
            content = content.Replace("\r\n", "\n");
            await File.WriteAllTextAsync(Path.Combine(_requestDirectory, "run_code.sh"), content);
        }

        // mounted to /code/requests/{requestId} directory in container
        await fileService.CreateCodeFile(code, template, language, _requestDirectory);
        await fileService.CreateTestCasesFile(testCases, _requestDirectory);
        await fileService.CreateExpectedOutputFile(testCases, _requestDirectory);

        try
        {
            await CreateAndStartContainer(language);
            var logs = await ExecuteCodeInContainer(runTimeLimit, testCases.Count);
            return await CalculateResult(testCases, logs);
        }
        catch (CodeExecutionException) { throw; }
        catch (Exception ex)
        {
            throw new CodeExecutionException($"Error while running testcases. InnerException: {ex.Message}", ex);
        }
        finally
        {
            if (Directory.Exists(_requestDirectory))
                Directory.Delete(_requestDirectory, true);

            if (_containerId != null)
                await _dockerClient.Containers.RemoveContainerAsync(
                    _containerId, new ContainerRemoveParameters { Force = true });
        }
    }

    private async Task CreateAndStartContainer(Language language)
    {
        var image = language switch
        {
            Language.py => Helper.PythonCompiler,
            Language.cpp => Helper.CppCompiler,
            Language.cs => Helper.CSharpCompiler,
            _ => throw new ArgumentException("Unsupported language")
        };

        var binds = new List<string>();
        string hostExecDir = _configuration["HOST_EXECUTION_DIR"];
        bool isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        
        if (!string.IsNullOrEmpty(hostExecDir) && !isRunningInContainer)
        {
            // If HOST_EXECUTION_DIR is set and we're not in a container, we assume we are running on host (e.g., local debug or tests)
            // and need to bind mount the local path.
            binds.Add($"{_requestDirectory}:/code/requests/{_requestId}");
        }
        else
        {
            // If HOST_EXECUTION_DIR is missing or we're in a container, we should use the shared named volume 'judge_data'.
            binds.Add("judge_data:/code");
        }

        var createContainerResponse = await _dockerClient.Containers.CreateContainerAsync(
            new CreateContainerParameters
            {
                HostConfig = new HostConfig
                {
                    Binds = binds,
                    NetworkMode = "none",
                    Memory = 256 * 1024 * 1024,
                    NanoCPUs = 1000000000,
                    Privileged = false,
                    AutoRemove = false
                },
                Image = image,
                Cmd = new[] { "tail", "-f", "/dev/null" }  // no apt-get installs
            });

        _containerId = createContainerResponse.ID;
        await _dockerClient.Containers.StartContainerAsync(
            _containerId, new ContainerStartParameters());
    }

    private async Task<string> ExecuteCodeInContainer(decimal timeLimit, int testCaseCount)
    {
        var execCreateResponse = await _dockerClient.Exec.ExecCreateContainerAsync(
            _containerId,
            new ContainerExecCreateParameters
            {
                Cmd = new[] { "/usr/bin/bash", $"/code/requests/{_requestId}/run_code.sh", _requestId, timeLimit.ToString("F0") },
                AttachStdout = true,
                AttachStderr = true,
                Tty = false
            });

        using var stream = await _dockerClient.Exec.StartAndAttachContainerExecAsync(
            execCreateResponse.ID, tty: false);

        int outerTimeoutMs = (int)(timeLimit * 1000 * testCaseCount) + 10_000;
        using var cts = new CancellationTokenSource(outerTimeoutMs);

        try
        {
            var output = await stream.ReadOutputToEndAsync(cts.Token);
            return $"stdout: {output.stdout}, stderr: {output.stderr}";
        }
        catch (OperationCanceledException)
        {
            throw new CodeExecutionException("Script execution timed out at container level.");
        }
    }

    private async Task<object> CalculateResult(List<Testcase> testCases, string logs)
    {
        int total = testCases.Count;

        string error = await ReadContainerFile("error.txt");
        string runtime = await ReadContainerFile("runtime.txt");
        string tle = await ReadContainerFile("tle.txt");

        if (error.Contains("COMPILATIONFAILED") ||
            error.Contains("BUILDFAILED") ||
            error.Contains("UNSUPPORTEDLANGUAGE"))
        {
            string formattedError = error
                .Replace("COMPILATIONFAILED", "")
                .Replace("BUILDFAILED", "")
                .Replace("UNSUPPORTEDLANGUAGE", "")
                .Trim();

            return new CompilationErrorResponse
            {
                Message = formattedError,
                SubmissionResult = SubmissionResult.CompilationError,
                NumberOfPassedTestCases = 0,
                TotalTestcases = total
            };
        }

        if (tle.Contains("TIMELIMITEXCEEDED"))
        {
            int passed = ExtractPassed(tle);
            var failedCase = testCases.ElementAtOrDefault(passed);
            return new TimeLimitExceedResponse
            {
                SubmissionResult = SubmissionResult.TimeLimitExceeded,
                NumberOfPassedTestCases = passed,
                TotalTestcases = total,
                Input = failedCase?.Input,
                ExpectedOutput = failedCase?.Output
            };
        }

        if (runtime.Contains("WRONG_ANSWER"))
        {
            int passed = ExtractPassed(runtime);
            var (_, expected, got) = ParseWrongAnswer(runtime);
            var failedCase = testCases.ElementAtOrDefault(passed);
            return new WrongAnswerResponse
            {
                SubmissionResult = SubmissionResult.WrongAnswer,
                NumberOfPassedTestCases = passed,
                TotalTestcases = total,
                ActualOutput = got,
                ExpectedOutput = expected,
                Input = failedCase?.Input
            };
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            int passed = ExtractPassed(runtime);
            var failedCase = testCases.ElementAtOrDefault(passed);
            return new RunTimeErrorResponse
            {
                Message = error,
                SubmissionResult = SubmissionResult.RunTimeError,
                NumberOfPassedTestCases = passed,
                TotalTestcases = total,
                Input = failedCase?.Input,
                ExpectedOutput = failedCase?.Output
            };
        }

        if (runtime.Contains("ACCEPTED"))
            return new AcceptedResponse
            {
                SubmissionResult = SubmissionResult.Accepted,
                NumberOfPassedTestCases = total,
                TotalTestcases = total,
                ExecutionTime = ExtractMaxExecutionTime(runtime)
            };

        throw new CodeExecutionException($"Unrecognised verdict in runtime.txt. error.txt: {error}, logs: {logs}");
    }

    private async Task<string> ReadContainerFile(string filename)
    {
        string filePath = Path.Combine(_requestDirectory, filename);
        if (File.Exists(filePath))
        {
            return (await File.ReadAllTextAsync(filePath)).Trim();
        }
        return string.Empty;
    }

    private int ExtractPassed(string content)
    {
        var match = Regex.Match(content, @"Passed:\s*(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private decimal ExtractMaxExecutionTime(string runtime)
    {
        var matches = Regex.Matches(runtime, @"Testcase \d+ time: (\d+)ms");
        if (!matches.Any()) return 0;
        return matches.Max(m => decimal.Parse(m.Groups[1].Value));
    }

    private (int testcaseNum, string expected, string got) ParseWrongAnswer(string runtime)
    {
        int num = 0;
        var numMatch = Regex.Match(runtime, @"Testcase:\s*(\d+)");
        if (numMatch.Success) int.TryParse(numMatch.Groups[1].Value, out num);

        var expMatch = Regex.Match(runtime,
            @"--- Expected ---\s*(.*?)\s*--- Got ---", RegexOptions.Singleline);
        var gotMatch = Regex.Match(runtime,
            @"--- Got ---\s*(.*?)$", RegexOptions.Singleline);

        return (
            num,
            expMatch.Success ? expMatch.Groups[1].Value.Trim() : string.Empty,
            gotMatch.Success ? gotMatch.Groups[1].Value.Trim() : string.Empty
        );
    }
}