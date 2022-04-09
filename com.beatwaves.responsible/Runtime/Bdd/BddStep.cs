using Responsible.State;

namespace Responsible.Bdd
{
	internal class BddStep<T> : IBddStep<T>
	{
		private readonly ITestInstruction<T> instruction;

		public BddStep(ITestInstruction<T> instruction)
		{
			this.instruction = instruction;
		}

		public ITestOperationState<T> CreateState() => this.instruction.CreateState();
	}
}
