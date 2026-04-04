namespace CoreJudge.Domain.Premitives
{
    public class CodeExecutionException : Exception
    {
        public CodeExecutionException(string message) : base(message) { }
        public CodeExecutionException(string message, Exception inner) : base(message, inner) { }
    }
}
