using System;
using Responsible.State;

namespace Responsible.TestResponders
{
	internal abstract class TestResponderBase<T> : ITestResponder<T>
	{
		private readonly Func<IOperationState<IOperationState<T>>> createState;

		protected TestResponderBase(Func<IOperationState<IOperationState<T>>> createState)
		{
			this.createState = createState;
		}

		public IOperationState<IOperationState<T>> CreateState() => this.createState();
	}
}