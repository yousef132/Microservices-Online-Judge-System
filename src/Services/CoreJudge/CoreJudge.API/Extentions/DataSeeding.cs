using CoreJudge.Domain.Models;
using CoreJudge.Domain.Models.Entities;
using CoreJudge.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CoreJudge.API.Extentions;

public static class DataSeeding
{
    public static async Task SeedDataAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<MassTransit.IPublishEndpoint>();
        var guid = Guid.NewGuid();


        var Contests = await db.Contests.ToListAsync();

        //if (Contests.Any())
        //    return;
        var problems = ConstructProblems(guid);
        var contest = new Contest
        {
            ContestSetterId = guid,
            Name = "Sample Contest",
            StartDate = DateTime.UtcNow.AddHours(-10),
            EndDate = DateTime.UtcNow.AddHours(-8),
            Problems = problems

        };

        db.Contests.Add(contest);
        await db.SaveChangesAsync();
        foreach (var problem in problems)
        {
            await bus.Publish(new CoreJudge.Domain.Events.ProblemCreatedEvent
            {
                Difficulty = problem.Difficulty.ToString(),
                ProblemId = problem.Id,
                Title = problem.Name
            });
        }
    }


    //    static Problem ConstructProblem(Guid guid)
    //    {
    //        return new Problem
    //        {
    //            Name = "Sample Problem 1",
    //            Description = "Write a function to find the maximum product of two words that have no common characters.",
    //            Difficulty = Difficulty.Easy,
    //            ProblemTopics = new List<ProblemTopic>
    //                    {
    //                        new ProblemTopic {
    //                        Topic = new Topic
    //                        {
    //                            Name = "Bit Manipulation",
    //                        }},
    //                        new ProblemTopic {  Topic = new Topic
    //                        {
    //                            Name = "Arrays",
    //                        }}
    //                    },
    //            MemoryLimit = Domain.Premitives.MemoryLimit.Lowest,
    //            ProblemSetterId = guid,
    //            RunTimeLimit = 1,
    //            ContestPoints = Domain.Premitives.ContestPoints.Level14,

    //            Testcases = new List<Testcase>
    //                    {
    //                        // Test case 1: Basic case with valid pair
    //                        new Testcase
    //                        {
    //                            Input = "4\nabcd\nwxyz\nab\nxy\n",
    //                            Output = "16"
    //                        },

    //                        // Test case 2: No valid pairs (all share characters)
    //                        new Testcase
    //                        {
    //                            Input = "3\na\nab\nabc\n",
    //                            Output = "0"
    //                        },

    //                        // Test case 3: Multiple valid pairs, return max
    //                        new Testcase
    //                        {
    //                            Input = "5\nabc\ndef\nghi\nab\nxy\n",
    //                            Output = "9"
    //                        },

    //                        // Test case 4: Single word (can't form pair)
    //                        new Testcase
    //                        {
    //                            Input = "1\nhello\n",
    //                            Output = "0"
    //                        },

    //                        // Test case 5: Large words
    //                        new Testcase
    //                        {
    //                            Input = "4\nabcdefghij\nklmnopqrst\nabc\nklm\n",
    //                            Output = "100"
    //                        },

    //                        // Test case 6: Words with common characters
    //                        new Testcase
    //                        {
    //                            Input = "4\nabc\nbcd\ncde\ndef\n",
    //                            Output = "9"
    //                        },

    //                        // Test case 7: Words with different lengths
    //                        new Testcase
    //                        {
    //                            Input = "4\na\nbc\ndef\nghij\n",
    //                            Output = "12"  // 
    //                        }
    //                    },

    //            LanguagesTemplages = new List<ProblemLangeuageTemplates>
    //                    {
    //                        new ProblemLangeuageTemplates
    //                {
    //                    Language = Domain.Premitives.Language.cpp,
    //                    StartingPoint = "// [USER_CODE_START]",
    //                    UserCodeTemplate = @"class Solution {
    //public:
    //    int maxProduct(vector<string>& words) {
    //        // Write your solution here

    //    }
    //};",
    //                    UserCodeWrapper = @"#include <iostream>
    //#include <vector>
    //#include <string>
    //#include <unordered_map>
    //#include <algorithm>
    //#include <bitset>
    //using namespace std;

    //// [USER_CODE_START]

    //int main() {
    //    int n;
    //    cin >> n;
    //    vector<string> words(n);
    //    for(int i = 0; i < n; i++) {
    //        cin >> words[i];
    //    }

    //    Solution sol;
    //    int result = sol.maxProduct(words);
    //    cout << result << endl;

    //    return 0;
    //}"
    //                },
    //                        new ProblemLangeuageTemplates
    //                {
    //                    Language = Domain.Premitives.Language.py,
    //                    StartingPoint = "# [USER_CODE_START]",
    //                    UserCodeTemplate = @"class Solution:
    //                        def maxProduct(self, words: List[str]) -> int:
    //                            # Write your solution here
    //                            pass",
    //                    UserCodeWrapper = @"from typing import List
    //                    import sys

    //                    # [USER_CODE_START]

    //                    if __name__ == '__main__':
    //                        n = int(sys.stdin.readline())
    //                        words = []
    //                        for _ in range(n):
    //                            words.append(sys.stdin.readline().strip())

    //                        sol = Solution()
    //                        result = sol.maxProduct(words)
    //                        print(result)"
    //                                    },
    //                        new ProblemLangeuageTemplates
    //                                    {
    //                                        Language = Domain.Premitives.Language.java,
    //                                        StartingPoint = "// [USER_CODE_START]",
    //                                        UserCodeTemplate = @"class Solution {
    //                        public int maxProduct(String[] words) {
    //                            // Write your solution here

    //                        }
    //                    }",
    //                    UserCodeWrapper = @"import java.util.*;
    //                                        import java.io.*;

    //                                        // [USER_CODE_START]

    //                                        public class Main {
    //                                            public static void main(String[] args) throws Exception {
    //                                                BufferedReader br = new BufferedReader(new InputStreamReader(System.in));
    //                                                int n = Integer.parseInt(br.readLine());
    //                                                String[] words = new String[n];
    //                                                for(int i = 0; i < n; i++) {
    //                                                    words[i] = br.readLine();
    //                                                }

    //                                                Solution sol = new Solution();
    //                                                int result = sol.maxProduct(words);
    //                                                System.out.println(result);
    //                                            }
    //                                        }"
    //                }
    //                    },

    //            Submissions = new List<Submission>
    //                    {
    //                        new Submission
    //                        {
    //                            AttemperId = guid,
    //                            Code = @"class Solution {
    //                            public:
    //                                int maxProduct(vector<string>& words) {
    //                                    int n = words.size();
    //                                    vector<int> masks(n, 0);

    //                                    // Create bitmask for each word
    //                                    for(int i = 0; i < n; i++) {
    //                                        for(char c : words[i]) {
    //                                            masks[i] |= 1 << (c - 'a');
    //                                        }
    //                                    }

    //                                    // Find maximum product
    //                                    int maxProduct = 0;
    //                                    for(int i = 0; i < n; i++) {
    //                                        for(int j = i + 1; j < n; j++) {
    //                                            if((masks[i] & masks[j]) == 0) {
    //                                                int product = words[i].length() * words[j].length();
    //                                                maxProduct = max(maxProduct, product);
    //                                            }
    //                                        }
    //                                    }

    //                                    return maxProduct;
    //                                }
    //                            };",
    //                            Language = Domain.Premitives.Language.cpp,
    //                            Result = Domain.Premitives.SubmissionResult.Accepted,
    //                            SubmissionDate = DateTime.UtcNow,
    //                            SubmitMemory = 256,
    //                            SubmitTime = 1000,
    //                        }
    //                    }
    //        };

    //    }

    static List<Problem> ConstructProblems(Guid guid)
    {
        return new List<Problem>
    {
        // 1. TWO SUM
        new Problem
        {
            Name = "Two Sum",
            Description = "Find indices of two numbers that add up to a target.",
            Difficulty = Difficulty.Easy,
            ProblemSetterId = guid,
            RunTimeLimit = 1,
            Testcases = new List<Testcase>
            {
                new() { Input = "4\n2 7 11 15\n9\n", Output = "0 1\n" },
                new() { Input = "3\n3 2 4\n6\n", Output = "1 2\n" },
                new() { Input = "2\n3 3\n6\n", Output = "0 1\n" },
                new() { Input = "4\n1 5 8 2\n10\n", Output = "2 3\n" },
                new() { Input = "5\n-1 -2 -3 -4 -5\n-8\n", Output = "2 4\n" }
            },
            LanguagesTemplages = new List<ProblemLangeuageTemplates>
            {
                new() {
                    Language = Domain.Premitives.Language.cpp,
                    StartingPoint = "// [USER_CODE_START]",
                    UserCodeTemplate = "class Solution {\npublic:\n    vector<int> twoSum(vector<int>& nums, int target) {\n        \n    }\n};",
                    UserCodeWrapper = @"#include <iostream>
                                        #include <vector>
                                        #include <unordered_map>
                                        using namespace std;
                                        
                                        // [USER_CODE_START]
                                        
                                        int main() {
                                            int t;
                                            if(!(cin >> t)) return 0;
                                            while(t--) {
                                                int n, target;
                                                if(!(cin >> n)) break;
                                                vector<int> nums(n);
                                                for(int i = 0; i < n; i++) cin >> nums[i];
                                                cin >> target;
                                                Solution sol;
                                                vector<int> res = sol.twoSum(nums, target);
                                                if(res.size() >= 2) cout << res[0] << "" "" << res[1] << ""\n"";
                                                cout << ""---DONE---"" << ""\n"";
                                                cout.flush();
                                            }
                                            return 0;
                                        }"
                }
            }
        },

        // 2. VALID PARENTHESES
        new Problem
        {
            Name = "Valid Parentheses",
            Description = "Determine if the input string of brackets is valid.",
            Difficulty = Difficulty.Easy,
            ProblemSetterId = guid,
            RunTimeLimit = 1,
            Testcases = new List<Testcase>
            {
                new() { Input = "()\n", Output = "true\n" },
                new() { Input = "()[]{}\n", Output = "true\n" },
                new() { Input = "(]\n", Output = "false\n" },
                new() { Input = "([)]\n", Output = "false\n" },
                new() { Input = "{[]}\n", Output = "true\n" }
            },
            LanguagesTemplages = new List<ProblemLangeuageTemplates>
            {
                new() {
                    Language = Domain.Premitives.Language.cpp,
                    StartingPoint = "// [USER_CODE_START]",
                    UserCodeTemplate = "class Solution {\npublic:\n    bool isValid(string s) {\n        \n    }\n};",
                    UserCodeWrapper = @"#include <iostream>
                    #include <string>
                    #include <stack>
                    using namespace std;
                    
                    // [USER_CODE_START]
                    
                    int main() {
                        int t;
                        if(!(cin >> t)) return 0;
                        while(t--) {
                            string s;
                            if(!(cin >> s)) break;
                            Solution sol;
                            cout << (sol.isValid(s) ? ""true"" : ""false"") << ""\n"";
                            cout << ""---DONE---"" << ""\n"";
                            cout.flush();
                        }
                        return 0;
                    }"
                }
            }
        },

        // 3. REVERSE LIST
        new Problem
        {
            Name = "Reverse List",
            Description = "Reverse an array of integers.",
            Difficulty = Difficulty.Easy,
            ProblemSetterId = guid,
            RunTimeLimit = 1,
            Testcases = new List<Testcase>
            {
                new() { Input = "5\n1 2 3 4 5\n", Output = "5 4 3 2 1\n" },
                new() { Input = "2\n1 2\n", Output = "2 1\n" },
                new() { Input = "1\n10\n", Output = "10\n" },
                new() { Input = "3\n0 0 1\n", Output = "1 0 0\n" },
                new() { Input = "4\n11 22 33 44\n", Output = "44 33 22 11\n" }
            },
            LanguagesTemplages = new List<ProblemLangeuageTemplates>
            {
                new() {
                    Language = Domain.Premitives.Language.cpp,
                    StartingPoint = "// [USER_CODE_START]",
                    UserCodeTemplate = "class Solution {\npublic:\n    vector<int> reverseList(vector<int>& nums) {\n        \n    }\n};",
                    UserCodeWrapper = @"#include <iostream>
                    #include <vector>
                    using namespace std;
                    
                    // [USER_CODE_START]
                    
                    int main() {
                        int t;
                        if(!(cin >> t)) return 0;
                        while(t--) {
                            int n;
                            if(!(cin >> n)) break;
                            vector<int> v(n);
                            for(int i = 0; i < n; i++) cin >> v[i];
                            Solution sol;
                            vector<int> res = sol.reverseList(v);
                            for(int i = 0; i < res.size(); i++) {
                                cout << res[i] << (i == res.size() - 1 ? """" : "" "");
                            }
                            cout << ""\n"";
                            cout << ""---DONE---"" << ""\n"";
                            cout.flush();
                        }
                        return 0;
                    }"
                }
            }
        }
    };
    }
}
