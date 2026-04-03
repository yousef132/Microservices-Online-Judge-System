using CoreJudge.Domain.Premitives;

namespace CoreJudge.Domain.Models
{
    public class ProblemLangeuageTemplates
    {
        public int Id { get; set; }
        // Template for the user code, it will be used to generate the user code file and run it in the container
        public string UserCodeTemplate { get; set; } = default!;
        //wrapper for user code, create object from user class and call it's functon (if user changed the thier name) will break the code,
        // get all testcases inputs one by one, call user function with passing input to it
        public string UserCodeWrapper { get; set; } = default!;
        public string StartingPoint { get; set; } = default!; // Starting point for setting user code class in the wrapper 
        public Language Language { get; set; } = default!;
        public int ProblemId { get; set; } = default!;
        public Problem Problem { get; set; } = default!;
    }
}
