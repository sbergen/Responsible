using System;
using Responsible.State;

namespace Responsible.TestWaitConditions
{
	internal abstract class TestWaitConditionBase<T> : ITestWaitCondition<T>
	{
		private readonly Func<IOperationState<T>> createState;

		protected TestWaitConditionBase(Func<IOperationState<T>> state)
		{
			this.createState = state;
		}

		public IOperationState<T> CreateState() => this.createState();
	}
}