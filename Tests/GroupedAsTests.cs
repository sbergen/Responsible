using System;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class GroupedAsTests : ResponsibleTestBase
	{
		private bool executed;
		private ITestInstruction<object> groupedAsInstruction;

		[SetUp]
		public void SetUp()
		{
			this.executed = false;
			var execute = Do("Execute", () => executed = true);
			this.groupedAsInstruction = execute.GroupedAs("Wrapper");
		}

		[Test]
		public void Execution_ExecutesInnerInstruction()
		{
			this.groupedAsInstruction.ToTask(this.Executor);

			Assert.IsTrue(executed);
		}

		[Test]
		public void StateString_ContainsExpectedContent_WhenNotRun()
		{
			var stateString = this.groupedAsInstruction.CreateState().ToString();

			StateAssert.StringContainsInOrder(stateString)
				.NotStarted("Wrapper")
				.Details("  ")
				.NotStarted("Execute");
		}

		[Test]
		public void StateString_ContainsExpectedContent_WhenCompleted()
		{
			var state = this.groupedAsInstruction.CreateState();
			state.ToTask(this.Executor);

			StateAssert.StringContainsInOrder(state.ToString())
				.Completed("Wrapper")
				.Details("  ")
				.Completed("Execute");
		}

		[Test]
		public void OperationStack_DoesNotContainGroupedAs()
		{
			var state = Responsibly
				.Do(
					"Throw",
					() => throw new Exception("Test exception"))
				.GroupedAs("Test group")
				.CreateState();

			state.ToTask(this.Executor);

			StringAssert.DoesNotContain(
				$"[{nameof(TestInstruction.GroupedAs)}]",
				state.ToString());
		}
	}
}
