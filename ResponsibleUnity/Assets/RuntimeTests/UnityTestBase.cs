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
			this.Executor = new UnityTestInstructionExecutor();
		}

		[TearDown]
		public void TearDownUnityTest()
		{
			this.Executor.Dispose();
		}
	}
}
