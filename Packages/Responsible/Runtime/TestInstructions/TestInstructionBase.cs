using System;
using Responsible.State;

namespace Responsible.TestInstructions
{
	internal abstract class TestInstructionBase<T> : ITestInstruction<T>
	{
		private readonly Func<IOperationState<T>> createState;

		protected TestInstructionBase(Func<IOperationState<T>> createState)
		{
			this.createState = createState;
		}

		public IOperationState<T> CreateState() => this.createState();
	}
}