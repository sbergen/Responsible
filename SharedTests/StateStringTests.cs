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
		public void StateString_TruncatesException_WhenLong()
		{
			var notIncluded = "Not included";
			var message = new string('x', 100) + notIncluded;
			var state = Responsibly.Do(
					"Fail",
					() => throw new Exception(message))
				.CreateState();

			state.ToTask(this.Executor); // Complete task

			var stateString = state.ToString();
			StateAssert.StringContainsInOrder(stateString)
				.Failed("Fail")
				.Details("xxxxxxx")
				.Nowhere(notIncluded);
		}
	}
}
