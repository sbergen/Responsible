using System;
using Responsible.State;

namespace Responsible.TestResponders
{
	internal abstract class TestResponderBase<T> : ITestResponder<T>
	{
		private readonly Func<ITestOperationState<ITestOperationState<T>>> createState;

		protected TestResponderBase(Func<ITestOperationState<ITestOperationState<T>>> createState)
		{
			this.createState = createState;
		}

		public ITestOperationState<ITestOperationState<T>> CreateState() => this.createState();
	}
}