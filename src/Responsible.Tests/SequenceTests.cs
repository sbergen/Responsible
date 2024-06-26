using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using static Responsible.Responsibly;

namespace Responsible.Tests
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
					DoAndReturn("Set completed1", () => completed1 = true).BoxResult(),
					DoAndReturn("Set completed2", () => completed2 = true).BoxResult(),
					DoAndReturn("Set completed3", () => completed3 = true).BoxResult(),
				})
				.ToTask(this.Executor);

			(completed1, completed2, completed3).Should().Be((true, true, true));
		}

		[Test]
		public async Task Sequence_StopsExecution_WhenErrorEncountered()
		{
			var completed1 = false;
			var completed2 = false;

			var task = TestInstruction
				.Sequence(new[]
				{
					DoAndReturn("Set completed1", () => completed1 = true).BoxResult(),
					Do("Throw error", () => throw new Exception()),
					DoAndReturn("Set completed2", () => completed2 = true).BoxResult(),
				})
				.ToTask(this.Executor);

			(completed1, completed2).Should().Be((true, false));
			(await AwaitFailureExceptionForUnity(task)).Should().NotBeNull();
		}
	}
}
