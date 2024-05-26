using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class WaitForFramesTests : ResponsibleTestBase
	{
		[Test]
		public void WaitForFrames_CompletesAfterTimeout()
		{
			var task = WaitForFrames(2).ToTask(this.Executor);

			task.IsCompleted.Should().BeFalse();
			this.AdvanceDefaultFrame(); // This frame
			task.IsCompleted.Should().BeFalse();
			this.AdvanceDefaultFrame(); // First frame
			task.IsCompleted.Should().BeFalse();
			this.AdvanceDefaultFrame(); // Second frame
			task.IsCompleted.Should().BeTrue();
		}

		[Test]
		public void WaitForFrames_CompletesAfterThisFrame_WithZeroFrames()
		{
			var task = WaitForFrames(0).ToTask(this.Executor);
			task.IsCompleted.Should().BeFalse();
			this.AdvanceDefaultFrame(); // This frame
			task.IsCompleted.Should().BeTrue();
		}

		[Test]
		public void WaitForFrames_ContainsCorrectStatusInDescription()
		{
			var state = WaitForFrames(0).CreateState();
			var description = @"WAIT FOR 0 FRAME(S)";
			StateAssert.StringContainsInOrder(state.ToString()).NotStarted(description);
			state.ToTask(this.Executor);
			StateAssert.StringContainsInOrder(state.ToString()).Waiting(description);
			this.AdvanceDefaultFrame();
			StateAssert.StringContainsInOrder(state.ToString()).Completed(description);
		}
	}
}
