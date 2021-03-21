using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class WaitForAllOfTests : ResponsibleTestBase
	{
		[Test]
		public void WaitForAllOf_Completes_AfterAllComplete()
		{
			var fulfilled1 = false;
			var fulfilled2 = false;
			var fulfilled3 = false;

			bool Id(bool val) => val;

			var task = WaitForAllOf(
					WaitForConditionOn("cond 1", () => fulfilled1, Id),
					WaitForConditionOn("cond 2", () => fulfilled2, Id),
					WaitForConditionOn("cond 3", () => fulfilled3, Id))
				.ExpectWithinSeconds(10)
				.ToTask(this.Executor);

			Assert.IsFalse(task.IsCompleted);
			this.AdvanceDefaultFrame();

			fulfilled1 = true;
			Assert.IsFalse(task.IsCompleted);
			this.AdvanceDefaultFrame();

			fulfilled2 = true;
			Assert.IsFalse(task.IsCompleted);
			this.AdvanceDefaultFrame();

			fulfilled3 = true;
			Assert.IsFalse(task.IsCompleted);
			this.AdvanceDefaultFrame();

			Assert.AreEqual(
				new[] { true, true, true },
				task.AssertSynchronousResult());
		}

		[Test]
		public void WaitForAllOf_Completes_WhenSynchronouslyMet()
		{
			var result = WaitForAllOf(ImmediateTrue, ImmediateTrue, ImmediateTrue)
				.ExpectWithinSeconds(10)
				.ToTask(this.Executor)
				.AssertSynchronousResult();

			Assert.AreEqual(
				new[] { true, true, true },
				result);
		}
	}
}
