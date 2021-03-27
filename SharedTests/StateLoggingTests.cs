using NUnit.Framework;
using Responsible.State;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class StateLoggingTests : ResponsibleTestBase
	{
		private int result;
		private ITestOperationState<int> state;

		[SetUp]
		public void SetUp()
		{
			this.result = -1;
			this.state = WaitForConditionOn(
				"Wait for value",
				() => this.result,
				val => val != -1)
				.ExpectWithinSeconds(1)
				.CreateState();
		}

		[Test]
		public void InitialState_ProducesCorrectOutput()
		{
			var stateString = this.state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.NotStarted("Wait for value EXPECTED WITHIN");
		}

		[Test]
		public void ToTask_ProducesCorrectOutput_AfterCompletion()
		{
			this.state.ToTask(this.Executor);
			this.result = 42;
			this.AdvanceDefaultFrame();

			var stateString = this.state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Completed("Wait for value EXPECTED WITHIN");
		}
	}
}
