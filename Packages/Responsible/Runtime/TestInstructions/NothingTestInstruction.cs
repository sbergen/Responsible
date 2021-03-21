using Responsible.State;

namespace Responsible.TestInstructions
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
