using Responsible.State;

namespace Responsible.TestWaitConditions
{
	internal class BoxedWaitCondition<T> : TestWaitConditionBase<object>
	{
		public BoxedWaitCondition(ITestWaitCondition<T> condition)
			: base(() => new BoxedOperationState<T, object>(
				condition.CreateState(), value => (object)value))
		{
		}
	}
}
