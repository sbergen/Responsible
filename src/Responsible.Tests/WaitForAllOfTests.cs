using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests
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
					WaitForPredicate("cond 1", () => fulfilled1, Id),
					WaitForPredicate("cond 2", () => fulfilled2, Id),
					WaitForPredicate("cond 3", () => fulfilled3, Id))
				.ExpectWithinSeconds(10)
				.ToTask(this.Executor);

			task.IsCompleted.Should().BeFalse();
			this.AdvanceDefaultFrame();

			fulfilled1 = true;
			task.IsCompleted.Should().BeFalse();
			this.AdvanceDefaultFrame();

			fulfilled2 = true;
			task.IsCompleted.Should().BeFalse();
			this.AdvanceDefaultFrame();

			fulfilled3 = true;
			task.IsCompleted.Should().BeFalse();
			this.AdvanceDefaultFrame();

			task.AssertSynchronousResult()
				.Should().BeEquivalentTo(new[] { true, true, true });
		}

		[Test]
		public void WaitForAllOf_Completes_WhenSynchronouslyMet()
		{
			var result = WaitForAllOf(ImmediateTrue, ImmediateTrue, ImmediateTrue)
				.ExpectWithinSeconds(10)
				.ToTask(this.Executor)
				.AssertSynchronousResult();

			result.Should().BeEquivalentTo(new[] { true, true, true });
		}

		[Test]
		public async Task WaitForAllOf_Completes_WhenOneHasError()
		{
			var canThrow = false;

			var expectedException = new Exception("Test exception");
			var task = WaitForAllOf(
					Never.BoxResult(),
					WaitForCondition(
						"Throw",
						() =>
						{
							if (canThrow)
							{
								throw expectedException;
							}

							return canThrow;
						}))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			task.IsCompleted.Should().BeFalse();
			task.IsFaulted.Should().BeFalse();

			canThrow = true;
			this.AdvanceDefaultFrame();

			var exception = await AwaitFailureExceptionForUnity(task);
			exception.InnerException.Should().BeSameAs(expectedException);
		}

		[Test]
		public void WaitForAllOf_Description_MatchesExpected()
		{
			var state = WaitForAllOf(
					WaitForCondition("First", () => false),
					WaitForCondition("Second", () => false))
				.CreateState();

			StateAssert.StringContainsInOrder(state.ToString())
				.NotStarted("WAIT FOR ALL OF")
				.NotStarted("First")
				.NotStarted("Second");
		}
	}
}
