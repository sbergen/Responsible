using Responsible.State;

namespace Responsible.TestWaitConditions
{
	internal class BoxedWaitCondition<T> : TestWaitConditionBase<object>
	where T : struct
	{
		public BoxedWaitCondition(ITestWaitCondition<T> condition)
			: base(() => new BoxedOperationState<T>(condition.CreateState())!)
		{
		}
	}
}
