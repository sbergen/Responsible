using System;
using NUnit.Framework;
using Responsible.NoRx;
using static Responsible.NoRx.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class SequenceTests : ResponsibleTestBase
	{
		[Test]
		public void Sequence_ExecutesAll_WhenNoErrors()
		{
			var completed1 = false;
			var completed2 = false;
			var completed3 = false;

			TestInstruction
				.Sequence(new[]
				{
					DoAndReturn("Set completed1", () => completed1 = true).AsNothingInstruction(),
					DoAndReturn("Set completed2", () => completed2 = true).AsNothingInstruction(),
					DoAndReturn("Set completed3", () => completed3 = true).AsNothingInstruction(),
				})
				.ToTask(this.Executor);

			Assert.AreEqual(
				(true, true, true),
				(completed1, completed2, completed3));
		}

		[Test]
		public void Sequence_StopsExecution_WhenErrorEncountered()
		{
			var completed1 = false;
			var completed2 = false;

			var task = TestInstruction
				.Sequence(new[]
				{
					DoAndReturn("Set completed1", () => completed1 = true).AsNothingInstruction(),
					Do("Throw error", () => throw new Exception()).AsNothingInstruction(),
					DoAndReturn("Set completed2", () => completed2 = true).AsNothingInstruction(),
				})
				.ToTask(this.Executor);

			Assert.AreEqual(
				(true, false),
				(completed1, completed2));
			Assert.IsNotNull(GetAssertionException(task));
		}
	}
}
