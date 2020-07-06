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
		public readonly ITestInstruction<T> Instruction;

		public UnitTestInstruction(ITestInstruction<T> instruction)
		{
			this.Instruction = instruction;
		}

		public IObservable<Unit> Run(RunContext runContext) =>
			this.Instruction.Run(runContext).AsUnitObservable();
	}
}