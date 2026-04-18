using System.Linq;
using System.Threading.Tasks;
using CodeSphere.Domain.Abstractions;
using CodeSphere.Domain.Abstractions.Services;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreJudge.IntegrationTests
{
    public class CodeExecutionTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public CodeExecutionTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            
            // Set required environment variables for host execution running through Docker
            var basePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.AppContext.BaseDirectory, "../../../../../src"));
            var scriptPath = System.IO.Path.Combine(basePath, "Services", "CoreJudge", "CoreJudge.Domain", "Premitives", "run_code.sh");
            
            System.Environment.SetEnvironmentVariable("HOST_EXECUTION_SCRIPT", scriptPath);
            System.Environment.SetEnvironmentVariable("RUN_CODE_SCRIPT_PATH", scriptPath);
            System.Environment.SetEnvironmentVariable("HOST_EXECUTION_DIR", System.IO.Path.GetTempPath());
        }
        
        [Fact]
        public async Task ExecuteCodeAsync_TwoSumProblem_ShouldReturnAcceptedResponse()
        {
             // Arrange
            using var scope = _factory.Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var executionService = scope.ServiceProvider.GetRequiredService<IExecutionService>();

            // Problem 1 is seeded as "Two Sum" with C++ template
            var language = Language.cpp;
            var problemId = 1;
            
            var problem = await unitOfWork.ProblemRepository.GetProblemIncludingContestAndTestcases(problemId, language);
            
            Assert.NotNull(problem);
            Assert.NotEmpty(problem.Testcases);
            
            var template = problem.LanguagesTemplages.FirstOrDefault();
            Assert.NotNull(template);

            // Using the correct code solution for Two Sum
            string code = @"
            class Solution {
            public:
                vector<int> twoSum(vector<int>& nums, int target) {
                    unordered_map<int, int> map;
                    for (int i = 0; i < nums.size(); i++) {
                        int complement = target - nums[i];
                        if (map.find(complement) != map.end()) {
                            return {map[complement], i};
                        }
                        map[nums[i]] = i;
                    }
                    return {};
                }
            };";

            // Act
            var result = await executionService.ExecuteCodeAsync(
                code,
                template,
                language,
                problem.Testcases.ToList(),
                problem.RunTimeLimit);

            // Assert
            Assert.NotNull(result);
            var baseResponse = result as BaseSubmissionResponse;
            Assert.NotNull(baseResponse);
            
            // Should accurately solve the problem and complete all testcases
            Assert.Equal(SubmissionResult.Accepted, baseResponse.SubmissionResult);
            
            if (result is AcceptedResponse acceptedRes)
            {
                Assert.True(acceptedRes.ExecutionTime >= 0);
            }
        }

        [Fact]
        public async Task ExecuteCodeAsync_ValidParenthesesProblem_ShouldReturnAcceptedResponse()
        {
             // Arrange
            using var scope = _factory.Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var executionService = scope.ServiceProvider.GetRequiredService<IExecutionService>();

            // Problem 2 is seeded as "Valid Parentheses" with C++ template
            var language = Language.cpp;
            var problemId = 2;
            
            var problem = await unitOfWork.ProblemRepository.GetProblemIncludingContestAndTestcases(problemId, language);
            
            Assert.NotNull(problem);
            Assert.NotEmpty(problem.Testcases);
            
            var template = problem.LanguagesTemplages.FirstOrDefault();
            Assert.NotNull(template);

            // Using the correct code solution for Valid Parentheses
            string code = @"
class Solution {
public:
    bool isValid(string s) {
        stack<char> st;
        for(char c : s) {
            if(c == '(' || c == '{' || c == '[') {
                st.push(c);
            } else {
                if(st.empty()) return false;
                char top = st.top();
                if(c == ')' && top != '(') return false;
                if(c == '}' && top != '{') return false;
                if(c == ']' && top != '[') return false;
                st.pop();
            }
        }
        return st.empty();
    }
};";

            // Act
            var result = await executionService.ExecuteCodeAsync(
                code,
                template,
                language,
                problem.Testcases.ToList(),
                problem.RunTimeLimit);

            // Assert
            Assert.NotNull(result);
            var baseResponse = result as BaseSubmissionResponse;
            Assert.NotNull(baseResponse);
            
            // Should accurately solve the problem and complete all testcases
            Assert.Equal(SubmissionResult.Accepted, baseResponse.SubmissionResult);
        }
    }
}
