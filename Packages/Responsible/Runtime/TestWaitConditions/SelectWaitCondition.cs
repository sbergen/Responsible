using System;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestWaitConditions
{
	internal class SelectWaitCondition<T1, T2> : TestWaitConditionBase<T2>
	{
		public SelectWaitCondition(ITestWaitCondition<T1> condition, Func<T1, T2> selector, SourceContext sourceContext)
			: base(() => new SelectOperationState<T1,T2>(condition.CreateState(), selector, sourceContext))
		{
		}
	}
}