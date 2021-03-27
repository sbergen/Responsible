using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class WaitForSecondsTests : ResponsibleTestBase
	{
		[Test]
		public void WaitForSeconds_Completes_AfterTimeout()
		{
			var task = WaitForSeconds(2).ToTask(this.Executor);
			Assert.IsFalse(task.IsCompleted);
			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsFalse(task.IsCompleted);
			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void WaitForSeconds_ContainsCorrectStatusInDescription()
		{
			var state = WaitForSeconds(1).CreateState();
			var description = "WAIT FOR 0:00:01";
			StateAssert.StringContainsInOrder(state.ToString()).NotStarted(description);
			state.ToTask(this.Executor);
			StateAssert.StringContainsInOrder(state.ToString()).Waiting(description);
			this.TimeProvider.AdvanceFrame(OneSecond);
			StateAssert.StringContainsInOrder(state.ToString()).Completed(description);
		}
	}
}
