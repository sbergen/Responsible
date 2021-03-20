using System;
using Responsible.NoRx.State;

namespace Responsible.NoRx.TestInstructions
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
