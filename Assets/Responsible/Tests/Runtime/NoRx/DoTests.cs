using NUnit.Framework;
using Responsible.NoRx;
using static Responsible.NoRx.Responsibly;

namespace Responsible.Tests.Runtime.NoRx
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

			Assert.IsFalse(executed, "Should not execute before ToTask is called");

			var task = instruction.ToTask(this.Executor);
			Assert.IsTrue(executed, "Should execute when ToTask is called");
			Assert.AreEqual(42, task.Result);
		}

		[Test]
		public void Do_Executes_WhenToTaskIsCalled()
		{
			var executed = false;
			var instruction = Do("Set executed", () => { executed = true; });

			Assert.IsFalse(executed, "Should not execute before ToTask is called");

			var unused = instruction.ToTask(this.Executor);
			Assert.IsTrue(executed, "Should execute when ToTask is called");
		}
	}
}
