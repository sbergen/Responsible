using System;
using Responsible.State;

namespace Responsible.TestInstructions
{
	internal abstract class TestInstructionBase<T> : ITestInstruction<T>
	{
		private readonly Func<ITestOperationState<T>> createState;

		protected TestInstructionBase(Func<ITestOperationState<T>> createState)
		{
			this.createState = createState;
		}

		public ITestOperationState<T> CreateState() => this.createState();
	}
}
