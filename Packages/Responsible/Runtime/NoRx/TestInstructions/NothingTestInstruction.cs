using Responsible.NoRx.State;

namespace Responsible.NoRx.TestInstructions
{
	internal class NothingTestInstruction<T> : TestInstructionBase<Nothing>
	{
		public NothingTestInstruction(ITestInstruction<T> instruction)
			: base(() => new NothingOperationState<T, Nothing>(
				instruction.CreateState(),
				_ => Nothing.Default))
		{
		}
	}
}
