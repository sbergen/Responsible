using NUnit.Framework;
using Responsible.Unity;

namespace Responsible.UnityTests
{
	public class UnityTestBase
	{
		private readonly bool logErrors;

		protected TestInstructionExecutor Executor { get; private set; }

		protected UnityTestBase(bool logErrors = true)
		{
			this.logErrors = logErrors;
		}

		[SetUp]
		public void SetUpUnityTest()
		{
			this.Executor = new UnityTestInstructionExecutor(this.logErrors);
		}

		[TearDown]
		public void TearDownUnityTest()
		{
			this.Executor.Dispose();
		}
	}
}
