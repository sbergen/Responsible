using Responsible.NoRx.State;

namespace Responsible.NoRx.TestWaitConditions
{
	internal class NothingWaitCondition<T> : TestWaitConditionBase<Nothing>
	{
		public NothingWaitCondition(ITestWaitCondition<T> condition)
			: base(() => new NothingOperationState<T, Nothing>(
				condition.CreateState(), _ => Nothing.Default))
		{
		}
	}
}
