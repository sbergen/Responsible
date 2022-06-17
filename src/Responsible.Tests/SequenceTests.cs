using System;
using System.Threading.Tasks;
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

			Assert.AreEqual(
				(true, true, true),
				(completed1, completed2, completed3));
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

			Assert.AreEqual(
				(true, false),
				(completed1, completed2));
			Assert.IsNotNull(await AwaitFailureExceptionForUnity(task));
		}
	}
}
