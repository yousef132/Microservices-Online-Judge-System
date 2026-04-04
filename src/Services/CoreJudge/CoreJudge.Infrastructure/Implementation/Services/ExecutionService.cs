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
    private string _requestDirectory = null;
    private string _containerId = null;

    public ExecutionService(IFileService fileService)
    {
        _dockerClient = new DockerClientConfiguration(
            new Uri("tcp://localhost:2375")).CreateClient();

        _requestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
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
        // mounted to /code directory in container
        await fileService.CreateCodeFile(code, template, language, _requestDirectory);
        await fileService.CreateTestCasesFile(testCases, _requestDirectory);
        await fileService.CreateExpectedOutputFile(testCases, _requestDirectory);

        try
        {
            await CreateAndStartContainer(language);
            await ExecuteCodeInContainer(runTimeLimit, testCases.Count);
            return await CalculateResult(testCases);
        }
        catch (CodeExecutionException) { throw; }
        catch (Exception)
        {
            throw new CodeExecutionException("Error while running testcases.");
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

        string scriptFilePath = Helper.ScriptFilePath;

        var createContainerResponse = await _dockerClient.Containers.CreateContainerAsync(
            new CreateContainerParameters
            {
                HostConfig = new HostConfig
                {
                    Binds = new[]
                    {
                        $"{_requestDirectory}:/code",
                        $"{scriptFilePath}:/run_code.sh"
                    },
                    NetworkMode = "bridge",
                    Memory = 256 * 1024 * 1024,
                    AutoRemove = false
                },
                Image = image,
                Cmd = new[] { "tail", "-f", "/dev/null" }  // no apt-get installs
            });

        _containerId = createContainerResponse.ID;
        await _dockerClient.Containers.StartContainerAsync(
            _containerId, new ContainerStartParameters());
    }

    private async Task ExecuteCodeInContainer(decimal timeLimit, int testCaseCount)
    {
        var execCreateResponse = await _dockerClient.Exec.ExecCreateContainerAsync(
            _containerId,
            new ContainerExecCreateParameters
            {
                Cmd = new[] { "/usr/bin/bash", "/run_code.sh", timeLimit.ToString("F0") },
                AttachStdout = true,
                AttachStderr = true,
                Tty = false
            });

        using var stream = await _dockerClient.Exec.StartAndAttachContainerExecAsync(
            execCreateResponse.ID, tty: false);

        // Total timeout = per-testcase limit × count + buffer for compile/startup
        // to limit conatiner running time
        int outerTimeoutMs = (int)(timeLimit * 1000 * testCaseCount) + 10_000;
        using var cts = new CancellationTokenSource(outerTimeoutMs);

        try
        {
            await stream.ReadOutputToEndAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new CodeExecutionException("Script execution timed out at container level.");
        }
    }

    private async Task<object> CalculateResult(List<Testcase> testCases)
    {
        int total = testCases.Count;

        string error = await ReadContainerFile("error.txt");
        string runtime = await ReadContainerFile("runtime.txt");
        string tle = await ReadContainerFile("tle.txt");

        if (error.Contains("COMPILATIONFAILED") ||
            error.Contains("BUILDFAILED") ||
            error.Contains("UNSUPPORTEDLANGUAGE"))
            return new CompilationErrorResponse
            {
                Message = error,
                SubmissionResult = SubmissionResult.CompilationError,
                NumberOfPassedTestCases = 0,
                TotalTestcases = total
            };

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

        throw new CodeExecutionException("Unrecognised verdict in runtime.txt");
    }

    private async Task<string> ReadContainerFile(string filename)
    {
        var execResponse = await _dockerClient.Exec.ExecCreateContainerAsync(
            _containerId,
            new ContainerExecCreateParameters
            {
                Cmd = new[] { "cat", $"/tmp/verdict/{filename}" },
                AttachStdout = true,
                AttachStderr = true,
                Tty = false
            });

        using var stream = await _dockerClient.Exec.StartAndAttachContainerExecAsync(
            execResponse.ID, tty: false);

        var (stdout, _) = await stream.ReadOutputToEndAsync(CancellationToken.None);
        return stdout.Trim();
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