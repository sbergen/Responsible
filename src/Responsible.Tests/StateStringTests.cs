using System;
using NUnit.Framework;
using Responsible.Tests.Utilities;

namespace Responsible.Tests
{
	public class StateStringTests : ResponsibleTestBase
	{
		[Test]
		public void StateString_ContainsOnlyFirstLineOfException_WhenMultiline()
		{
			var firstLine = "First line";
			var secondLine = "Second line";
			var state = Responsibly.Do(
					"Fail",
					() => throw new Exception($"{firstLine}\n{secondLine}"))
				.CreateState();

			state.ToTask(this.Executor); // Complete task

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Failed("Fail")
				.Details(firstLine)
				.Nowhere(secondLine);
		}

		[Test]
		public void StateString_TruncatesExceptionAt100Chars()
		{
			var message = new string('x', 99) + "^~";
			var state = Responsibly.Do(
					"Fail",
					() => throw new Exception(message))
				.CreateState();

			state.ToTask(this.Executor); // Complete task

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Failed("Fail")
				.Details(@"xxxxxxx\^")
				.Nowhere("~");
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
			var noContextString = "No extra context provided";

			if (run)
			{
				state.ToTask(this.Executor);
				StringAssert.Contains(noContextString, state.ToString());
			}
			else
			{
				StringAssert.DoesNotContain(noContextString, state.ToString());
			}
		}
	}
}
