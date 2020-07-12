using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class UnitTestInstruction<T> : TestInstructionBase<Unit>
	{
		public UnitTestInstruction(ITestInstruction<T> instruction)
			: base(() => new UnitOperationState<T, Unit>(
				instruction.CreateState(),
				_ => Unit.Default))
		{
		}
	}
}