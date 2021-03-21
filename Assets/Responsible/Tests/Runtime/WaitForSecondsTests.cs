using NUnit.Framework;
using Responsible.NoRx;
using static Responsible.NoRx.Responsibly;

namespace Responsible.Tests.Runtime
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
			StringAssert.Contains("[ ]", state.ToString());
			state.ToTask(this.Executor);
			StringAssert.Contains("[.]", state.ToString());
			this.TimeProvider.AdvanceFrame(OneSecond);
			StringAssert.Contains("[✓]", state.ToString());
		}
	}
}
