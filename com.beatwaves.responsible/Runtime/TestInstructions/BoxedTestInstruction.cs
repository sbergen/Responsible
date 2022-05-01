using Responsible.State;

namespace Responsible.TestInstructions
{
	internal class BoxedTestInstruction<T> : TestInstructionBase<object?>
		where T : struct
	{
		public BoxedTestInstruction(ITestInstruction<T> instruction)
			: base(() => new BoxedOperationState<T>(instruction.CreateState()))
		{
		}
	}
}
