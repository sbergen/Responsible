using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class UnitWaitCondition<T> : TestWaitConditionBase<Unit>
	{
		public UnitWaitCondition(ITestWaitCondition<T> condition)
			: base(() => new UnitOperationState<T, Unit>(
				condition.CreateState(),
				_ => Unit.Default))
		{
		}
	}
}