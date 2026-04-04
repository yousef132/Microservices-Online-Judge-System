using CodeSphere.Domain.Abstractions.Services;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace CodeSphere.Infrastructure.Implementation.Services
{
    public class FileService : IFileService
    {

        private readonly IWebHostEnvironment webHostEnvironment;
        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }
        public bool CheckFileExtension(IFormFile file, Language language)
        {
            var extension = System.IO.Path.GetExtension(file.FileName);
            if (extension == null)
                return false;

            extension = extension[1..];

            bool found = false;
            // find the supported language and compare it with the file extension
            foreach (Language lan in Enum.GetValues(typeof(Language)))
                if (lan == language && extension == lan.ToString())
                    found = true;

            return found;
        }
        public async Task<string> ReadFileAsync(string filePath)
               => await System.IO.File.ReadAllTextAsync(filePath);


        public async Task CreateTestCasesFile(List<Testcase> testCases, string directory)
        {
            var sb = new StringBuilder();
            foreach (var tc in testCases)
            {
                sb.Append(tc.Input.TrimEnd().Replace("\r\n", "\n").Replace("\r", "\n"));
                sb.Append("\n---END---\n");
            }

            await File.WriteAllTextAsync(
                Path.Combine(directory, "testcases.txt"),
                sb.ToString(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public async Task CreateExpectedOutputFile(List<Testcase> testCases, string directory)
        {
            var sb = new StringBuilder();
            foreach (var tc in testCases)
            {
                // Use \n explicitly — never AppendLine which writes \r\n on Windows
                sb.Append(tc.Output.TrimEnd().Replace("\r\n", "\n").Replace("\r", "\n"));
                sb.Append("\n---END---\n");
            }

            await File.WriteAllTextAsync(
                Path.Combine(directory, "expected.txt"),
                sb.ToString(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public async Task<string> CreateCodeFile(string code, ProblemLangeuageTemplates template, Language language, string requestDirectory)
        {
            code = template.UserCodeWrapper.Replace(template.StartingPoint, code);
            string testCasesPath = Path.Combine(requestDirectory, $"main.{language.ToString()}");

            await System.IO.File.WriteAllTextAsync(testCasesPath, code);
            return testCasesPath;
        }

        public async Task<string> ReadFile(IFormFile file)
        {
            string content;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                content = await reader.ReadToEndAsync();
            }
            return content;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string directory)
        {
            var serverPath = webHostEnvironment.WebRootPath;
            var extension = Path.GetExtension(file.FileName);
            var imagePath = $"/{directory}/{Guid.NewGuid().ToString().Replace("-", string.Empty)}{extension}";

            var fullPath = serverPath + imagePath;

            if (file.Length > 0)
            {
                try
                {
                    if (!Directory.Exists(serverPath))
                        // if directory not found then make one
                        Directory.CreateDirectory(serverPath);

                    using (FileStream stream = File.Create(fullPath))
                    {
                        await file.CopyToAsync(stream);
                        await stream.FlushAsync();
                        return imagePath;
                    }
                }
                catch (Exception ex)
                {
                    return "FailedToUploadImage";
                    throw;
                }

            }
            else
                return "NoImage";
        }
    }
}
