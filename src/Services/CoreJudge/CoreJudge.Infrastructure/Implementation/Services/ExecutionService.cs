using CodeSphere.Domain.Abstractions;
using CodeSphere.Domain.Abstractions.Services;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Text.RegularExpressions;


namespace CodeSphere.Infrastructure.Implementation.Services
{
    public class ExecutionService : IExecutionService
    {
        private readonly DockerClient _dockerClient;
        private readonly IFileService fileService;
        private readonly IUnitOfWork unitOfWork;
        private string _requestDirectory = null;
        private string _containerId = null;

        // the 4 files the shell script actually writes
        private string outputFile;
        private string errorFile;
        private string runtimeFile;
        private string tleFile;

        public ExecutionService(IFileService fileService, IUnitOfWork unitOfWork)
        {
            _dockerClient = new DockerClientConfiguration(
                new Uri("tcp://localhost:2375")).CreateClient();

            _requestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_requestDirectory);

            outputFile = Path.Combine(_requestDirectory, "output.txt");
            errorFile = Path.Combine(_requestDirectory, "error.txt");
            runtimeFile = Path.Combine(_requestDirectory, "runtime.txt");
            tleFile = Path.Combine(_requestDirectory, "tle.txt");

            this.fileService = fileService;
            this.unitOfWork = unitOfWork;
        }

        public async Task<object> ExecuteCodeAsync(string code, ProblemLangeuageTemplates template, Language language, List<Testcase> testCases, decimal runTimeLimit)
        {
            await fileService.CreateCodeFile(code, template, language, _requestDirectory);


            try
            {
                await CreateAndStartContainer(language);

                // Write ALL testcases at once, delimited by ---END---
                await fileService.CreateTestCasesFile(testCases, _requestDirectory);

                // Write ALL expected outputs at once, same delimiter
                await fileService.CreateExpectedOutputFile(testCases, _requestDirectory);

                // Run the script ONCE
                await ExecuteCodeInContainer(runTimeLimit);

                // Read verdict from files
                return await CalculateResult(testCases);
            }
            catch (Exception ex)
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
        private async Task<object> CalculateResult(List<Testcase> testCases)
        {
            int total = testCases.Count;
            string error = await ReadFile(errorFile);
            string runtime = await ReadFile(runtimeFile);
            string tle = await ReadFile(tleFile);

            // 1. Compilation failure
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

            // 2. TLE
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

            // 3. Wrong answer
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

            // 4. Runtime error
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

            // 5. Accepted
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

            var createContainerResponse = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                HostConfig = new HostConfig
                {
                    Binds = new[] { $"{_requestDirectory}:/code", $"{scriptFilePath}:/run_code.sh" },
                    NetworkMode = "bridge",
                    Memory = 256 * 1024 * 1024,
                    AutoRemove = false
                },
                Image = image,
                Cmd = new[] { "sh", "-c", "apt-get update && apt-get install -y time && tail -f /dev/null" }, // Install time package

            });

            _containerId = createContainerResponse.ID;
            await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
        }

        private async Task ExecuteCodeInContainer(decimal timeLimit)
        {

            ////// running the shell script to execute code and return output | Errors | etc.
            //string command = Helper.CreateExecuteCodeCommand(_containerId, timeLimit);

            //// Start the process to execute the command
            ////docker exec b4a5525c894d6ea84e81f15b439d5aec8760882a39dd04e192e91335e74a8ca0 /usr/bin/bash /run_code.sh 1s
            //using (var process = new System.Diagnostics.Process())
            //{
            //    process.StartInfo = new System.Diagnostics.ProcessStartInfo
            //    {
            //        FileName = "cmd.exe",
            //        Arguments = $"/C {command}",
            //        RedirectStandardOutput = true,
            //        RedirectStandardError = true,
            //        UseShellExecute = false,
            //        CreateNoWindow = true
            //    };
            //    try
            //    {
            //        process.Start();
            //        process.WaitForExit();

            //    }
            //    catch (Exception ex)
            //    {
            //        // Handle exceptions
            //        throw new CodeExecutionException("Error While Executing Client Code !!");
            //    }
            //}

            // Create the exec instance inside the already-running container
            var execCreateResponse = await _dockerClient.Exec.ExecCreateContainerAsync(
                _containerId,
                new ContainerExecCreateParameters
                {
                    Cmd = new[] { "/usr/bin/bash", "/run_code.sh", timeLimit.ToString("F0") },
                    AttachStdout = true,
                    AttachStderr = true,
                    Tty = false
                }
            );

            // Attach and start
            using var stream = await _dockerClient.Exec.StartAndAttachContainerExecAsync(
                execCreateResponse.ID,
                tty: false
            );

            // Outer timeout = 2× the per-testcase limit + 10s buffer for startup/compile
            int outerTimeoutMs = (int)(timeLimit * 1000 * 2) + 10_000;
            using var cts = new CancellationTokenSource(outerTimeoutMs);

            try
            {
                // This BLOCKS until the script fully exits — files are ready after this line
                var (stdout, stderr) = await stream.ReadOutputToEndAsync(cts.Token);

                if (!string.IsNullOrWhiteSpace(stderr))
                    await File.AppendAllTextAsync(errorFile, $"\n[docker exec stderr]: {stderr}");
            }
            catch (OperationCanceledException)
            {
                await File.AppendAllTextAsync(tleFile,
                    $"TIMELIMITEXCEEDED\nTestcase: 1\nPassed: 0\nTime consumed (ms): {outerTimeoutMs}\n");

                throw new CodeExecutionException("Script execution timed out at container level.");
            }

        }

        private async Task<string> ReadFile(string filepath)
        {
            if (!File.Exists(filepath)) return string.Empty;
            return (await File.ReadAllTextAsync(filepath)).Trim();
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

            var expMatch = Regex.Match(runtime, @"--- Expected ---\s*(.*?)\s*--- Got ---", RegexOptions.Singleline);
            var gotMatch = Regex.Match(runtime, @"--- Got ---\s*(.*?)$", RegexOptions.Singleline);

            return (
                num,
                expMatch.Success ? expMatch.Groups[1].Value.Trim() : string.Empty,
                gotMatch.Success ? gotMatch.Groups[1].Value.Trim() : string.Empty
            );
        }

    }
}