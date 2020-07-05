using System;
using NUnit.Framework;
using UniRx;
using static Responsible.RF;

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

			TestInstructionExtensions
				.Sequence(new[]
				{
					Do(() => completed1 = true).AsUnitInstruction(),
					Do(() => completed2 = true).AsUnitInstruction(),
					Do(() => completed3 = true).AsUnitInstruction(),
				})
				.Execute()
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
			Exception error = null;

			TestInstructionExtensions
				.Sequence(new[]
				{
					Do(() => completed1 = true).AsUnitInstruction(),
					Do(() => throw new Exception()).AsUnitInstruction(),
					Do(() => completed2 = true).AsUnitInstruction(),
				})
				.Execute()
				.Subscribe(_ => { }, e => error = e);

			Assert.AreEqual(
				(true, false),
				(completed1, completed2));
			Assert.IsInstanceOf<AssertionException>(error);
		}
	}
}