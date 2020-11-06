using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using Packages.Rider.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Responsible.Tools
{
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public static class ContinuousIntegration
	{
		[MenuItem("CI/Lint")]
		public static void Lint()
		{
			var solution = SyncSolution();
			var (status, stdout, stderr) = RunCommand(
				"sh", "-l", "scripts/lint.sh", solution);

			Debug.Log($"Linting output:\n{stdout}");

			if (status != 0)
			{
				Debug.LogError($"Linting failed ({status}):\n{stderr}");
			}
		}

		private static string SyncSolution()
		{
			var projectGeneration = new ProjectGeneration();
			projectGeneration.Sync();
			return projectGeneration.SolutionFile();
		}

		[SuppressMessage(
			"ReSharper",
			"PossibleNullReferenceException",
			Justification = "Fail hard on errors")]
		private static (int status, string stdout, string stderr) RunCommand(string command, params string[] args)
		{
			var repositoryDir = new DirectoryInfo(Application.dataPath).Parent.FullName;

			var processInfo = new ProcessStartInfo
			{
				FileName = command,
				Arguments = string.Join(" ", args),
				WorkingDirectory = repositoryDir,
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