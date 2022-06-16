using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Responsible.EditorSetup
{
	public static class TestRunner
	{
		[MenuItem("CI/Run tests %#T")]
		public static async void RunTests()
		{
			var failCount = -1;
			try
			{
				var callbacks = new Callbacks();

				TestRunnerApi.RegisterTestCallback(callbacks);
				TestRunnerApi.ExecuteTestRun(new ExecutionSettings(new Filter
				{
					testMode = TestMode.EditMode | TestMode.PlayMode,
				}));

				var results = await callbacks.Results;

				// No public way to write the test-run xml element :(
				var resultsWriterType = typeof(TestRunnerApi).Assembly
					.GetType("UnityEditor.TestTools.TestRunner.Api.ResultsWriter");
				var writeMethod = resultsWriterType.GetMethod(
					"WriteResultToFile",
					BindingFlags.Instance | BindingFlags.Public);
				var resultsWriter = Activator.CreateInstance(resultsWriterType);
				// ReSharper disable once PossibleNullReferenceException
				writeMethod.Invoke(resultsWriter, new object[]
				{
					results,
					$"{ContinuousIntegration.RepositoryPath}/TestResults/test-results.xml",
				});

				failCount = results.FailCount;
			}
			finally
			{
				if (Application.isBatchMode)
				{
					EditorApplication.Exit(failCount);
				}
			}
		}

		private class Callbacks : ICallbacks
		{
			private readonly TaskCompletionSource<ITestResultAdaptor> completionSource =
				new TaskCompletionSource<ITestResultAdaptor>();

			public Task<ITestResultAdaptor> Results => this.completionSource.Task;

			public void RunStarted(ITestAdaptor testsToRun)
			{
			}

			public void RunFinished(ITestResultAdaptor result)
			{
				this.completionSource.SetResult(result);
			}

			public void TestStarted(ITestAdaptor test)
			{
			}

			public void TestFinished(ITestResultAdaptor result)
			{
			}
		}
	}
}
