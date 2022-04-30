using System;
using System.Linq;
using JetBrains.Annotations;
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
				var stateString = state.ToString();

				StringAssert.Contains(details, stateString);
			}
			else
			{
				var stateString = state.ToString();

				StringAssert.DoesNotContain(details, stateString);
				AssertNoEmptyLines(stateString);
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
				var stateString = state.ToString();

				StringAssert.Contains(noContextText, stateString);
				StringAssert.Contains(considerText, stateString);
			}
			else
			{
				var stateString = state.ToString();

				StringAssert.DoesNotContain(noContextText, stateString);
				StringAssert.DoesNotContain(considerText, stateString);
				AssertNoEmptyLines(stateString);
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

		[AssertionMethod]
		private static void AssertNoEmptyLines(string str)
		{
			var emptyLineCount = str
				.Split('\n')
				.Select(s => s.Trim())
				.Count(string.IsNullOrEmpty);
			Assert.AreEqual(
				0,
				emptyLineCount,
				$"String should not contain empty lines, was:\n{str}");
		}
	}
}
