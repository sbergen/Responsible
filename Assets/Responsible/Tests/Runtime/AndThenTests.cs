using NUnit.Framework;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class AndThenTests : ResponsibleTestBase
	{
		[Test]
		public void AndThen_CompletesWhenAllComplete_WithReadySecondCondition()
		{
			var cond1 = false;
			var cond2 = false;

			var task = WaitForCondition("First", () => cond1)
				.AndThen(WaitForCondition("Second", () =>
				{
					Assert.IsTrue(cond1);
					return cond2;
				}))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted);

			cond1 = true;
			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted);

			cond2 = true;
			this.AdvanceDefaultFrame();
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void AndThen_CompletesWhenAllComplete_WithDeferredSecondCondition()
		{
			var cond1 = false;
			var cond2 = false;

			var task = WaitForCondition("First", () => cond1)
				.AndThen(_ => WaitForCondition("Second", () =>
				{
					Assert.IsTrue(cond1);
					return cond2;
				}))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted);

			cond1 = true;
			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted);

			cond2 = true;
			this.AdvanceDefaultFrame();
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void AndThen_TimesOutOnFirst_WithReadySecondCondition()
		{
			var task = Never.AndThen(ImmediateTrue).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetAssertionException(task));
		}

		[Test]
		public void AndThen_TimesOutOnFirst_WithDeferredSecondCondition()
		{
			var task = Never.AndThen(_ => ImmediateTrue).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetAssertionException(task));
		}

		[Test]
		public void AndThen_TimesOutOnSecond_WithReadySecondCondition()
		{
			var task = ImmediateTrue.AndThen(Never).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetAssertionException(task));
		}

		[Test]
		public void AndThen_TimesOutOnSecond_WithDeferredSecondCondition()
		{
			var task = ImmediateTrue.AndThen(_ => Never).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.TimeProvider.AdvanceFrame(OneSecond);
			Assert.IsNotNull(GetAssertionException(task));
		}
	}
}
