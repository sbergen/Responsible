using Responsible.State;
using UniRx;

namespace Responsible.TestResponders
{
	internal class UnitTestResponder<T> : TestResponderBase<Unit>
	{
		public UnitTestResponder(ITestResponder<T> responder)
		: base(() => new UnitOperationState<ITestOperationState<T>,ITestOperationState<Unit>>(
			responder.CreateState(),
			TestOperationState.AsUnitOperationState))
		{
		}
	}
}