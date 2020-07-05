using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	/// <summary>
	/// Provides "type erasure" for test instructions. We can't use Object as the generic type,
	/// as value types don't derive from it (no Any-type in C#)
	/// </summary>
	internal class UnitTestInstruction<T> : ITestInstruction<Unit>
	{
		private readonly ITestInstruction<T> instruction;

		public UnitTestInstruction(ITestInstruction<T> instruction)
		{
			this.instruction = instruction;
		}

		public IObservable<Unit> Run(RunContext runContext) =>
			this.instruction.Run(runContext).AsUnitObservable();
	}
}