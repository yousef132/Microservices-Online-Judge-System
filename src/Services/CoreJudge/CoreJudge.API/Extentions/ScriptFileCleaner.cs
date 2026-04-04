using System.Text;

namespace CoreJudge.API.Extentions;

public static class ScriptFileCleaner
{
    // Plan / Pseudocode:
    // 1. Validate that the file at 'path' exists; throw FileNotFoundException if it does not.
    // 2. Read all bytes from the file asynchronously.
    // 3. Try to decode bytes as UTF-8; if decoding fails, fall back to the system default encoding.
    // 4. Remove any Unicode BOM character from the decoded content string.
    // 5. Normalize line endings to LF-only by replacing CRLF and CR with LF.
    // 6. If a shebang appears later in the file (index > 0), strip any leading content before the shebang.
    // 7. Ensure the script starts with a shebang; if not, prefix with "#!/usr/bin/env bash\n".
    // 8. Trim any leading whitespace.
    // 9. Write the cleaned content back to the file without emitting a UTF-8 BOM.
    // 10. Re-read the file bytes to verify that there is no BOM, CRLF, or stray BOM character.
    // 11. If verification fails, collect and include the first few bytes in the exception message.
    // 12. Log success to the console when cleaning completes.

    /// <summary>
    /// Cleans a script file to ensure it is UTF-8 without BOM and uses LF line endings,
    /// guarantees a shebang at the top, and strips any leading garbage before the shebang.
    /// </summary>
    /// <param name="services">Service provider (unused, kept for extension method compatibility).</param>
    /// <param name="path">Path to the script file to clean.</param>
    public static async Task CleanScriptFile(this IServiceProvider services, string path)
    {
        // 1. Ensure file exists.
        if (!File.Exists(path))
            throw new FileNotFoundException($"Script not found at: {path}");

        // 2. Read all bytes from the file.
        byte[] bytes = await File.ReadAllBytesAsync(path);

        string content;
        try
        {
            // 3. Try decode as UTF-8 first.
            content = Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            // 3b. If decoding fails, fall back to the system default encoding.
            content = Encoding.Default.GetString(bytes);
        }

        // 4. Remove any explicit Unicode BOM character (U+FEFF) from the string.
        content = content.Replace("\uFEFF", "");

        // 5. Normalize Windows CRLF and old Mac CR line endings to LF-only.
        content = content.Replace("\r\n", "\n").Replace("\r", "\n");

        // 6. If a shebang occurs later in the file (index > 0),
        //    remove any content that appears before it (common if metadata was prepended).
        int shebangIndex = content.IndexOf("#!");
        if (shebangIndex > 0)
        {
            content = content.Substring(shebangIndex);
        }

        // 7. Ensure the script starts with a shebang; if not, prepend a standard bash shebang.
        if (!content.StartsWith("#!"))
        {
            content = "#!/usr/bin/env bash\n" + content;
        }

        // 8. Remove any leading whitespace that might precede the shebang after modifications.
        content = content.TrimStart();

        // 9. Write the cleaned content back to disk without emitting a UTF-8 BOM.
        await File.WriteAllTextAsync(
            path,
            content,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        );

        // 10. Re-read the file bytes to verify the file has no BOM and uses LF-only.
        byte[] verifyBytes = await File.ReadAllBytesAsync(path);

        // 11. Detect BOM by checking the first three bytes for the UTF-8 BOM pattern 0xEF,0xBB,0xBF.
        bool hasBom =
            verifyBytes.Length >= 3 &&
            verifyBytes[0] == 0xEF &&
            verifyBytes[1] == 0xBB &&
            verifyBytes[2] == 0xBF;

        // Decode verification bytes as UTF-8 for content checks.
        string verify = Encoding.UTF8.GetString(verifyBytes);

        // 12. If there's still a BOM, CR characters, or stray FEFF characters, throw with diagnostics.
        if (hasBom || verify.Contains('\r') || verify.Contains("\uFEFF"))
        {
            var firstBytes = string.Join(" ",
                verifyBytes.Take(10).Select(b => b.ToString("X2")));

            throw new InvalidOperationException(
                $"Script still has BOM/CRLF after cleaning: {path}\nFirst bytes: {firstBytes}");
        }

        // 13. Log success so callers can see the operation completed.
        Console.WriteLine("[startup] run_code.sh cleaned successfully — LF only, no BOM.");
    }

}