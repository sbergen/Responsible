using System;
using Responsible.NoRx.State;

namespace Responsible.NoRx.TestWaitConditions
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
