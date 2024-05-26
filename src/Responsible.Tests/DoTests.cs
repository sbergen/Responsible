using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class DoTests : ResponsibleTestBase
	{
		[Test]
		public void DoAndReturn_ExecutesWithCorrectResult_WhenToTaskIsCalled()
		{
			var executed = false;
			var instruction = DoAndReturn("Meaning of life",
				() =>
				{
					executed = true;
					return 42;
				});

			executed.Should()
				.BeFalse("the instruction should not execute before ToTask is called");

			var task = instruction.ToTask(this.Executor);
			executed.Should().BeTrue("the instruction should execute when ToTask is called");
			task.AssertSynchronousResult().Should().Be(42);
		}

		[Test]
		public void Do_Executes_WhenToTaskIsCalled()
		{
			var executed = false;
			var instruction = Do("Set executed", () => { executed = true; });

			executed.Should()
				.BeFalse("the instruction should not execute before ToTask is called");

			var unused = instruction.ToTask(this.Executor);
			executed.Should()
				.BeTrue("the instruction should execute when ToTask is called");
		}
	}
}
