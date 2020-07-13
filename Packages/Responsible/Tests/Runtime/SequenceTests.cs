using System;
using NUnit.Framework;
using UniRx;
using static Responsible.Responsibly;

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
					DoAndReturn("Set completed1", () => completed1 = true).AsUnitInstruction(),
					DoAndReturn("Set completed2", () => completed2 = true).AsUnitInstruction(),
					DoAndReturn("Set completed3", () => completed3 = true).AsUnitInstruction(),
				})
				.ToObservable()
				.Subscribe();

			Assert.AreEqual(
				(true, true, true),
				(completed1, completed2, completed3));
		}

		[Test]
		public void Sequence_StopsExecution_WhenErrorEncountered()
		{
			var completed1 = false;
			var completed2 = false;

			TestInstruction
				.Sequence(new[]
				{
					DoAndReturn("Set completed1", () => completed1 = true).AsUnitInstruction(),
					Do("Throw error", () => throw new Exception()).AsUnitInstruction(),
					DoAndReturn("Set completed2", () => completed2 = true).AsUnitInstruction(),
				})
				.ToObservable()
				.Subscribe(Nop, this.StoreError);

			Assert.AreEqual(
				(true, false),
				(completed1, completed2));
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}
	}
}