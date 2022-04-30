using System;
using NUnit.Framework;
using Responsible.State;
using Responsible.Tests.Utilities;

namespace Responsible.Tests
{
	public class StateStringTests : ResponsibleTestBase
	{
		[Test]
		public void StateString_ContainsProperlyIndentedException_WhenMultiline()
		{
			var state = Responsibly.Do(
					"Fail",
					() => throw new Exception("first line\nsecond line"))
				.CreateState();

			state.ToTask(this.Executor); // Complete task

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Failed("Fail")
				.Details("    System.Exception:")
				.Details("      first line")
				.Details("      second line");
		}

		[Test]
		public void ExtraContext_IsIncluded_WhenProvidedAndApplicable([Values] bool run)
		{
			var details = "details that should be included";
			var state = Responsibly
				.WaitForCondition(
					"Never",
					() => false,
					builder => builder.AddDetails(details))
				.ExpectWithinSeconds(1).CreateState();

			if (run)
			{
				state.ToTask(this.Executor);
				StringAssert.Contains(details, state.ToString());
			}
			else
			{
				StringAssert.DoesNotContain(details, state.ToString());
			}
		}

		[Test]
		public void ExtraContextHint_IsIncluded_WhenNotProvidedAndApplicable([Values] bool run)
		{
			var state = Never.ExpectWithinSeconds(1).CreateState();
			var noContextText = "No extra context provided";
			var considerText = "Consider using";

			if (run)
			{
				state.ToTask(this.Executor);
				StringAssert.Contains(noContextText, state.ToString());
				StringAssert.Contains(considerText, state.ToString());
			}
			else
			{
				StringAssert.DoesNotContain(noContextText, state.ToString());
				StringAssert.DoesNotContain(considerText, state.ToString());
			}
		}

		[Test]
		public void Multiline_Details_AreProperlyIndented()
		{
			var builder = new StateStringBuilder();
			builder.AddNestedDetails(
				"Description:",
				b => b.AddDetails("first line\nsecond line"));

			Assert.AreEqual(
				builder.ToString(),
				"Description:\n  first line\n  second line");
		}
	}
}
