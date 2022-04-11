using Responsible.State;

namespace Responsible.Bdd
{
	internal class BddStep : IBddStep
	{
		private readonly ITestInstruction<object> instruction;

		public BddStep(ITestInstruction<object> instruction)
		{
			this.instruction = instruction;
		}

		public ITestOperationState<object> CreateState() => this.instruction.CreateState();
	}
}
