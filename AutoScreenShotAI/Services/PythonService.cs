using System.Diagnostics;
using System.IO;
using System.Text;

namespace AutoScreenShotAI.Services
{
    public static class PythonService
    {
        public static async Task<string> RunScriptAsync(string scriptPath, string imagePath, string prompt)
        {
            return await Task.Run(() => RunScript(scriptPath, imagePath, prompt));
        }

        private static string RunScript(string scriptPath, string imagePath, string prompt)
        {
            string pythonExe = FindPython(scriptPath);
            string args = $"\"{scriptPath}\" \"{imagePath}\" \"{EscapeArg(prompt)}\"";

            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi) ?? throw new System.Exception("Failed to start Python process");
            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();

            if (!proc.WaitForExit(60000))
            {
                proc.Kill();
                return "Error: Python script timed out.";
            }

            if (!string.IsNullOrWhiteSpace(error))
                return $"[STDERR]\n{error}\n\n[STDOUT]\n{output}";
            return output.Trim();
        }

        private static string FindPython(string scriptPath)
        {
            string venv = Path.Combine(Path.GetDirectoryName(scriptPath)!, "venv", "Scripts", "python.exe");
            if (File.Exists(venv)) return venv;
            return "python";
        }

        private static string EscapeArg(string s) => s.Replace("\"", "\\\"");
    }
}