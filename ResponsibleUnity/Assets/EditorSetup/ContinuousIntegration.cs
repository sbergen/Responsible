using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Responsible.EditorSetup
{
    public static class ContinuousIntegration
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static readonly string RepositoryPath = new DirectoryInfo(Application.dataPath).Parent.Parent.FullName;
        private static readonly string DocFxDir = Path.Combine(RepositoryPath, "docfx");
        private static readonly string DocFxJsonPath = Path.Combine(DocFxDir, "docfx.json");

        /// <summary>
        /// Build documentation from within Unity,
        /// to make this easier to use with existing Unity-related CI infrastructure.
        /// Code references aren't properly gathered from loose .cs files,
        /// so we need to first generate the solution.
        /// </summary>
        [MenuItem("CI/Build Documentation %#D")]
        public static async void BuildDocumentation()
        {
	        var returnCode = 0;
            string stdout;

	        try
	        {
                // Mark directory as safe for git if running in GitHub Actions
                // See e.g. https://github.com/actions/runner/issues/2033
                const string githubWorkspace = "/github/workspace";
                if (RepositoryPath == githubWorkspace)
                {
                    (returnCode, stdout) = RunCommand(
                        workingDir: RepositoryPath,
                        command: "git",
                        "config", "--global", "--add", "safe.directory", githubWorkspace);

                    if (returnCode != 0)
                    {
                        Debug.LogError($"Setting git directory as safe failed:\n{stdout}");
                    }
                }

		        Debug.Log("Updating failure example...");
		        File.WriteAllText(
			        Path.Combine(DocFxDir, "failure.md"),
			        await DocumentationExample.CreateDocumentationFailure());

		        Debug.Log("Syncing solution...");
		        SyncSolution();

		        Debug.Log("Building documentation...");
		        (returnCode, stdout) = RunCommand(
			        workingDir: DocFxDir,
			        command: "docfx",
			        Quote(DocFxJsonPath), "--warningsAsErrors", "--force");

		        if (returnCode != 0)
		        {
			        Debug.LogError($"Building documentation failed:\n{stdout}");
		        }
		        else
		        {
			        Debug.Log($"Finished building documentation:\n{stdout}");
		        }
	        }
	        finally
			{
				if (Application.isBatchMode)
				{
					EditorApplication.Exit(returnCode);
				}
			}
        }

        [SuppressMessage(
            "ReSharper",
            "PossibleNullReferenceException",
            Justification = "Having to use reflection because of internal types :/")]
        private static string SyncSolution()
        {
            Type
                .GetType("Packages.Rider.Editor.RiderScriptEditor, Unity.Rider.Editor")
                .GetMethod("SyncSolution", BindingFlags.Static | BindingFlags.Public)
                .Invoke(null, Array.Empty<object>());

            return Path.Combine(
                new DirectoryInfo(Application.dataPath).Parent.FullName,
                "ResponsibleUnity.sln");
        }

        private static string Quote(string str) => $"\"{str}\"";

        private static (int status, string stdout) RunCommand(
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
            process.WaitForExit();
            return (process.ExitCode, stdout.Result);
        }
    }
}
