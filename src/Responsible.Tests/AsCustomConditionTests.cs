using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	internal sealed class AsCustomConditionTests : ResponsibleTestBase
	{
		private bool cond1;
		private bool cond2;

		[SetUp]
		public void SetUp()
		{
			this.cond1 = false;
			this.cond2 = false;
		}

		[Test]
		public void AsCustomCondition_CompletesLikeNormalCondition()
		{
			var task = this.WaitForBoth("Wait for both").ExpectWithinSeconds(1).ToTask(this.Executor);
			task.IsCompleted.Should().BeFalse("no conditions are met");

			cond1 = true;
			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeFalse("only the first condition is met");

			cond2 = true;
			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeTrue("both conditions are met");
		}

		[Test]
		public void AsCustomCondition_OnlyContainsStateStringForCustomCondition_WhenNotStarted()
		{
			var stateString = this.WaitForBoth("Wait for both").CreateState().ToString();
			StateAssert.StringContainsInOrder(stateString)
				.NotStarted("Wait for both");
			stateString.Should().NotContain("Condition 1");
		}

		[Test]
		public void AsCustomCondition_OnlyContainsStateStringForCustomCondition_WhenCompleted()
		{
			this.cond1 = this.cond2 = true;
			var state = this.WaitForBoth("Wait for both").CreateState();
			state.ToTask(this.Executor);

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Completed("Wait for both");
			stateString.Should().NotContain("Condition 1");
		}

		[Test]
		public void AsCustomCondition_OnlyContainsStateStringForCustomCondition_WhenNotCompleted()
		{
			this.cond1 = true;
			var state = this.WaitForBoth("Wait for both").CreateState();
			state.ToTask(this.Executor);

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Waiting("Wait for both");
			stateString.Should().NotContain("Condition 1");
		}

		[Test]
		public async Task AsCustomCondition_ContainsFullState_WhenTimedOut()
		{
			this.cond1 = true;
			var state = this.WaitForBoth("Wait for both").ExpectWithinSeconds(1).CreateState();
			var task = state.ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(TimeSpan.FromSeconds(2));

			var error = await AwaitFailureExceptionForUnity(task);
			var stateString = error.Message;
			StateAssert.StringContainsInOrder(stateString)
				.Failed("Wait for both")
				.Details("'Wait for both' was built from:")
				.Completed("Condition 1")
				.Canceled("Condition 2");
		}

		[Test]
		public void AsCustomCondition_ContainsOperationInStack_WhenFailed()
		{
			this.cond1 = true;
			var state = this.WaitWithError("Wait with error").CreateState();
			state.ToTask(this.Executor);
			this.AdvanceDefaultFrame();

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Failed("Wait with error")
				.Details($"[{nameof(WaitWithError)}]")
				.Failed("Condition with error");

		}

		private ITestWaitCondition<object> WaitForBoth(
			string description,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			WaitForCondition("Condition 1", () => this.cond1)
				.AndThen(WaitForCondition("Condition 2", () => this.cond2))
				.AsCustomCondition(description, memberName, sourceFilePath, sourceLineNumber);

		private ITestWaitCondition<object> WaitWithError(
			string description,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			WaitForCondition("Condition with error", () => throw new Exception("Test failure"))
				.AsCustomCondition(description, memberName, sourceFilePath, sourceLineNumber);
	}
}
