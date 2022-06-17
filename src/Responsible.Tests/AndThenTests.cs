using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests
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
		public async Task AndThen_TimesOutOnFirst_WithReadySecondCondition()
		{
			var task = Never.AndThen(ImmediateTrue).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(OneSecond);
			Assert.IsNotNull(await AwaitFailureException(task));
		}

		[Test]
		public async Task AndThen_TimesOutOnFirst_WithDeferredSecondCondition()
		{
			var task = Never.AndThen(_ => ImmediateTrue).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(OneSecond);
			Assert.IsNotNull(await AwaitFailureException(task));
		}

		[Test]
		public async Task AndThen_TimesOutOnSecond_WithReadySecondCondition()
		{
			var task = ImmediateTrue.AndThen(Never).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(OneSecond);
			Assert.IsNotNull(await AwaitFailureException(task));
		}

		[Test]
		public async Task AndThen_TimesOutOnSecond_WithDeferredSecondCondition()
		{
			var task = ImmediateTrue.AndThen(_ => Never).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(OneSecond);
			Assert.IsNotNull(await AwaitFailureException(task));
		}

		[Test]
		public async Task AndThen_Fails_IfContinuationConstructionThrows()
		{
			var expectedException = new Exception("Test exception");
			var task = ImmediateTrue
				.AndThen<bool, object>(_ => throw expectedException)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			var exception = await AwaitFailureException(task);
			Assert.AreSame(expectedException, exception.InnerException);
			StateAssert.StringContainsInOrder(exception.Message)
				.Completed("True")
				.Failed("...")
				.Details("Test exception");
		}

		[Test]
		public async Task AndThen_Fails_IfContinuationThrows()
		{
			var expectedException = new Exception("Test exception");
			var task = ImmediateTrue
				.AndThen(_ => WaitForCondition(
					"throw",
					() => throw expectedException))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			var exception = await AwaitFailureException(task);
			Assert.AreSame(expectedException, exception.InnerException);
			StateAssert.StringContainsInOrder(exception.Message)
				.Completed("True")
				.Failed("...")
				.Details("Test exception");
		}
	}
}
