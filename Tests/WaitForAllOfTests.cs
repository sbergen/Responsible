using System;
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

		[Test]
		public void WaitForAllOf_Completes_WhenOneHasError()
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
			Assert.IsFalse(task.IsCompleted);
			Assert.IsFalse(task.IsFaulted);

			canThrow = true;
			this.AdvanceDefaultFrame();

			var exception = GetFailureException(task);
			Assert.AreSame(expectedException, exception.InnerException);
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
