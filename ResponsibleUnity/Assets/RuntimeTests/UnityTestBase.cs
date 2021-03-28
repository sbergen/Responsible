using NUnit.Framework;
using Responsible.Unity;

namespace Responsible.UnityTests
{
	public class UnityTestBase
	{
		protected TestInstructionExecutor Executor { get; private set; }

		[SetUp]
		public void SetUpUnityTest()
		{
			// Logging errors really complicates testing failures.
			// We have error logging tests separately.
			this.Executor = new UnityTestInstructionExecutor(logErrors: false);
		}

		[TearDown]
		public void TearDownUnityTest()
		{
			this.Executor.Dispose();
		}
	}
}
