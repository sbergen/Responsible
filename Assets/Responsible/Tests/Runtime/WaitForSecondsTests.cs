using NUnit.Framework;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class WaitForSecondsTests : ResponsibleTestBase
	{
		[Test]
		public void WaitForSeconds_Completes_AfterTimeout()
		{
			var completed = false;
			using (WaitForSeconds(2).ToObservable(this.Executor).Subscribe(_ => completed = true))
			{
				Assert.IsFalse(completed);
				this.Scheduler.AdvanceBy(OneSecond);
				Assert.IsFalse(completed);
				this.Scheduler.AdvanceBy(OneSecond);
				Assert.IsTrue(completed);
			}
		}

		[Test]
		public void WaitForSeconds_ContainsCorrectStatusInDescription()
		{
			var state = WaitForSeconds(1).CreateState();
			StringAssert.Contains("[ ]", state.ToString());
			using (state.ToObservable(this.Executor).Subscribe())
			{
				StringAssert.Contains("[.]", state.ToString());
				this.Scheduler.AdvanceBy(OneSecond);
				StringAssert.Contains("[✓]", state.ToString());
			}
		}
	}
}