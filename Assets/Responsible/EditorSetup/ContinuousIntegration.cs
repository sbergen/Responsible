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
        private static readonly string ResharperDir = Path.Combine(RepositoryPath, "resharper");
        private static readonly string ResharperCache = Path.Combine(ResharperDir, ".cache");
        private static readonly string ResharperResults = Path.Combine(ResharperDir, "inspect.xml");
        private static readonly string ResharperStdout = Path.Combine(ResharperDir, "stdout.txt");
        private static readonly string ResharperStderr = Path.Combine(ResharperDir, "stderr.txt");
        private static readonly string DotSettingsFile = Path.Combine(RepositoryPath, "Responsible.sln.DotSettings");

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
                workingDir: DocFxDir,
                command: "docfx",
                Quote(DocFxJsonPath), "--warningsAsErrors", "--force");

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

        /// <summary>
        /// Run the JetBrains inspectcode tool from within Unity,
        /// to make this easier to use with existing Unity-related CI infrastructure.
        /// ReSharper requires Unity dlls, so this can't be run without Unity around.
        /// </summary>
        [MenuItem("CI/Inspect Code")]
        public static void InspectCode()
        {
            Debug.Log("Syncing solution...");
            var solution = SyncSolution();

            Debug.Log("Running inspections...");
            var (status, stdout, stderr) = RunCommand(
                workingDir: RepositoryPath,
                command: "inspectcode",
                Quote(solution),
                $"-o={Quote(ResharperResults)}",
                $"--caches-home={Quote(ResharperCache)}",
                "-s=WARNING",
                $"--profile={Quote(DotSettingsFile)}");

            File.WriteAllText(ResharperStdout, stdout);
            File.WriteAllText(ResharperStderr, stderr);

            if (status != 0)
            {
                Debug.LogError($"Inspecting code failed:\n{stderr}");
                throw new Exception($"Inspecting code failed ({status})");
            }
            else
            {
                Debug.Log($"Finished inspecting code:\n{stdout}");
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
