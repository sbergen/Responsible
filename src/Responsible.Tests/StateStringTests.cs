using System;
using System.Linq;
using FluentAssertions;
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
		public void BlankLines_ContainASpace_ForUnity()
		{
			var state = Responsibly
				.Do("Fail", () => throw new Exception())
				.CreateState();

			state.ToTask(this.Executor); // Complete task

			var lines = state.ToString()!.Split(Environment.NewLine);
			lines.Should().Contain(
				" ",
				"Blank lines should contain a space, so that Unity does not strip it");
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

				stateString.Should().Contain(details);
			}
			else
			{
				var stateString = state.ToString();

				stateString.Should().NotContain(details);
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

				stateString.Should().Contain(noContextText);
				stateString.Should().Contain(considerText);
			}
			else
			{
				var stateString = state.ToString();

				stateString.Should().NotContain(noContextText);
				stateString.Should().NotContain(considerText);
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

			builder.ToString().ReplaceLineEndings("\n").Should()
				.Be("Description:\n  first line\n  second line");
		}

		[AssertionMethod]
		private static void AssertNoEmptyLines(string str)
		{
			var emptyLineCount = str
				.Split('\n')
				.Select(s => s.Trim())
				.Count(string.IsNullOrEmpty);
			emptyLineCount.Should()
				.Be(0, $"String should not contain empty lines, was:\n{str}");
		}
	}
}
