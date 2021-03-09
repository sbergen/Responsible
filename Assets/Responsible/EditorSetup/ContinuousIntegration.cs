using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Packages.Rider.Editor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Responsible.EditorSetup
{
    public static class ContinuousIntegration
    {

        // ReSharper disable once PossibleNullReferenceException, will not happen
        private static readonly string RepositoryPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
        private static readonly string DocFxDir = Path.Combine(RepositoryPath, "docfx");
        private static readonly string DocFxJsonPath = Path.Combine(DocFxDir, "docfx.json");
        private static readonly string DocFxBinDir = Path.Combine(DocFxDir, "bin");
        private static readonly string DocFxExe = Path.Combine(DocFxBinDir, "docfx.exe");

        /// <summary>
        /// Build documentation from within Unity,
        /// to make this easier to use with existing Unity-related CI infrastructure.
        /// Code references aren't properly gathered from loose .cs files,
        /// so we need to first generate the solution.
        /// </summary>
        [MenuItem("CI/Build Documentation")]
        public static void BuildDocumentation()
        {
            Debug.Log("Syncing solution...");
            SyncSolution();

            Debug.Log("Building documentation...");
            var (status, stdout, _) = RunCommand(
                workingDir: DocFxBinDir,
                command: "mono",
                Quote(DocFxExe), Quote(DocFxJsonPath), "--warningsAsErrors");

            if (status != 0)
            {
                Debug.LogError($"Building documentation failed:\n{stdout}");
                throw new Exception($"Building documentation failed ({status})");
            }
            else
            {
                Debug.Log($"Finished building documentation:\n{stdout}");
            }
        }

        private static string SyncSolution()
        {
            var projectGeneration = new ProjectGeneration();
            projectGeneration.Sync();
            return projectGeneration.SolutionFile();
        }

        private static string Quote(string str) => $"\"{str}\"";

        private static (int status, string stdout, string stderr) RunCommand(
            string workingDir,
            string command,
            params string[] args)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = string.Join(" ", args),
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var process = Process.Start(processInfo);
            Assert.IsNotNull(process);
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            return (process.ExitCode, stdout.Result, stderr.Result);
        }
    }
}
