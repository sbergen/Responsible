using System;
using System.Threading.Tasks;
using FluentAssertions;
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
					cond1.Should().BeTrue();
					return cond2;
				}))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeFalse();

			cond1 = true;
			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeFalse();

			cond2 = true;
			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeTrue();
		}

		[Test]
		public void AndThen_CompletesWhenAllComplete_WithDeferredSecondCondition()
		{
			var cond1 = false;
			var cond2 = false;

			var task = WaitForCondition("First", () => cond1)
				.AndThen(_ => WaitForCondition("Second", () =>
				{
					cond1.Should().BeTrue();
					return cond2;
				}))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeFalse();

			cond1 = true;
			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeFalse();

			cond2 = true;
			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeTrue();
		}

		[Test]
		public async Task AndThen_TimesOutOnFirst_WithReadySecondCondition()
		{
			var task = Never.AndThen(ImmediateTrue).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(OneSecond);
			(await AwaitFailureExceptionForUnity(task)).Should().NotBeNull();
		}

		[Test]
		public async Task AndThen_TimesOutOnFirst_WithDeferredSecondCondition()
		{
			var task = Never.AndThen(_ => ImmediateTrue).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(OneSecond);
			(await AwaitFailureExceptionForUnity(task)).Should().NotBeNull();
		}

		[Test]
		public async Task AndThen_TimesOutOnSecond_WithReadySecondCondition()
		{
			var task = ImmediateTrue.AndThen(Never).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(OneSecond);
			(await AwaitFailureExceptionForUnity(task)).Should().NotBeNull();
		}

		[Test]
		public async Task AndThen_TimesOutOnSecond_WithDeferredSecondCondition()
		{
			var task = ImmediateTrue.AndThen(_ => Never).ExpectWithinSeconds(1).ToTask(this.Executor);
			this.Scheduler.AdvanceFrame(OneSecond);
			(await AwaitFailureExceptionForUnity(task)).Should().NotBeNull();
		}

		[Test]
		public async Task AndThen_Fails_IfContinuationConstructionThrows()
		{
			var expectedException = new Exception("Test exception");
			var task = ImmediateTrue
				.AndThen<bool, object>(_ => throw expectedException)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			var exception = await AwaitFailureExceptionForUnity(task);
			exception.InnerException.Should().BeSameAs(expectedException);
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

			var exception = await AwaitFailureExceptionForUnity(task);
			exception.InnerException.Should().BeSameAs(expectedException);
			StateAssert.StringContainsInOrder(exception.Message)
				.Completed("True")
				.Failed("throw")
				.Details("Test exception");
		}
	}
}
