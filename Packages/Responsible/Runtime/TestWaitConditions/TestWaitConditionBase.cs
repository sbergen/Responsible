using System;
using Responsible.State;

namespace Responsible.TestWaitConditions
{
	internal abstract class TestWaitConditionBase<T> : ITestWaitCondition<T>
	{
		private readonly Func<ITestOperationState<T>> createState;

		protected TestWaitConditionBase(Func<ITestOperationState<T>> state)
		{
			this.createState = state;
		}

		public ITestOperationState<T> CreateState() => this.createState();
	}
}
