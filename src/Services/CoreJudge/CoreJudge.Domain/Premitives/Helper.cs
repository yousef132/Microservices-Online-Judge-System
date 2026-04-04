
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CoreJudge.Domain.Premitives
{
    public static class Helper
    {
        public static string ScriptFilePath;
        public const string PythonCompiler = "python:3.11-trixie";
        public const string CppCompiler = "gcc:15-trixie";
        public const string CSharpCompiler = "mcr.microsoft.com/dotnet/sdk:5.0";

        public static string GenerateContestKey(int contestId)
        {
            return $"contest:{contestId}:standing";
        }
        public static string GenerateUserSubmissionsKey(Guid userId, int contestId)
        {
            // key contains userid and contest id 
            return $"user:{userId}:contest:{contestId}:submissions";
        }
        public static string CreateExecuteCodeCommand(string containerId, decimal timeLimit)
        {
            string runTimeLimit = $"{timeLimit}s";
            //string runMemoryLimit = $"{Math.Round(memoryLimit)}mb";  // Round to nearest integer to avoid any decimals

            return $"docker exec {containerId} /usr/bin/bash /run_code.sh {runTimeLimit}";
        }
        static Helper()
        {
            ScriptFilePath = SetScriptFilePath();
        }

        public static T DeserializeObject<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        public static IEnumerable<T> DeserializeCollection<T>(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Allows matching JSON property names with different cases
            };
            return JsonSerializer.Deserialize<IEnumerable<T>>(json, options);
        }


        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static string SetScriptFilePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo directoryInfo = new DirectoryInfo(currentDirectory);

            // Navigate up to the solution root
            while (directoryInfo != null && !DirectoryContainsFile(directoryInfo.FullName, "*.sln"))
            {
                directoryInfo = directoryInfo.Parent;
            }

            if (directoryInfo == null)
            {
                throw new Exception("Solution root not found.");
            }

            // Combine solution path with the Infrastructure project path
            string infrastructurePath = Path.Combine(directoryInfo.FullName, "Services", "CoreJudge", "CoreJudge.Domain", "Premitives", "run_code.sh");


            return infrastructurePath;

        }
        private static bool DirectoryContainsFile(string directoryPath, string searchPattern)
        {
            return Directory.GetFiles(directoryPath, searchPattern).Length > 0;
        }

        public static bool ValidateFile(Language fileType, decimal maxSizeInMb, decimal minSizeInMb, IFormFile file)
        {
            var extention = Path.GetExtension(file.FileName); // .cs,.cpp, .py, .java
            var size = file.Length;

            string requiredType = '.' + fileType.ToString();
            if (extention != requiredType)
                return false;

            if (size > maxSizeInMb * 1024 * 1024 || size < minSizeInMb * 1024 * 1024)
                return false;

            return true;
        }
    }
}
