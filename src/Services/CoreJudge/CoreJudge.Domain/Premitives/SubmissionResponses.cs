namespace CoreJudge.Domain.Premitives
{
    /// <summary>
    /// Base class for all submission result responses returned by the execution engine.
    /// </summary>
    public abstract class BaseSubmissionResponse
    {
        public SubmissionResult SubmissionResult { get; set; }
        public int NumberOfPassedTestCases { get; set; }
        public int TotalTestcases { get; set; }
    }

    public class AcceptedResponse : BaseSubmissionResponse
    {
        public AcceptedResponse() { SubmissionResult = SubmissionResult.Accepted; }
        public decimal ExecutionTime { get; set; }
    }

    public class WrongAnswerResponse : BaseSubmissionResponse
    {
        public WrongAnswerResponse() { SubmissionResult = SubmissionResult.WrongAnswer; }
        public string? Input { get; set; }
        public string? ExpectedOutput { get; set; }
        public string? ActualOutput { get; set; }
    }

    public class CompilationErrorResponse : BaseSubmissionResponse
    {
        public CompilationErrorResponse() { SubmissionResult = SubmissionResult.CompilationError; }
        public string? Message { get; set; }
    }

    public class RunTimeErrorResponse : BaseSubmissionResponse
    {
        public RunTimeErrorResponse() { SubmissionResult = SubmissionResult.RunTimeError; }
        public string? Message { get; set; }
        public string? Input { get; set; }
        public string? ExpectedOutput { get; set; }
    }

    public class TimeLimitExceedResponse : BaseSubmissionResponse
    {
        public TimeLimitExceedResponse() { SubmissionResult = SubmissionResult.TimeLimitExceeded; }
        public string? Input { get; set; }
        public string? ExpectedOutput { get; set; }
    }
}
