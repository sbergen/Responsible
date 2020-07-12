using Responsible.State;
using UniRx;

namespace Responsible.TestResponders
{
	internal class UnitTestResponder<T> : TestResponderBase<Unit>
	{
		public UnitTestResponder(ITestResponder<T> responder)
		: base(() => new UnitOperationState<IOperationState<T>,IOperationState<Unit>>(
			responder.CreateState(),
			OperationState.AsUnitOperationState))
		{
		}
	}
}