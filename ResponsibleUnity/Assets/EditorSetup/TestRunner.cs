
using System.Threading.Tasks;
using System.Xml;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

namespace Responsible.EditorSetup
{
	public static class TestRunner
	{
		[MenuItem("CI/Run tests %#T")]
		public static async void RunTests()
		{
			var callbacks = new Callbacks();

			TestRunnerApi.RegisterTestCallback(callbacks);
			TestRunnerApi.ExecuteTestRun(new ExecutionSettings(new Filter
			{
				testMode = TestMode.EditMode | TestMode.PlayMode,
			}));

			var results = await callbacks.Results;

			using (var writer = XmlWriter.Create(
				"test-results.xml",
				new XmlWriterSettings { Indent = true }))
			{
				results.ToXml().WriteTo(writer);
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
