using Responsible.NoRx.State;

namespace Responsible.NoRx.TestResponders
{
	internal class NothingTestResponder<T> : TestResponderBase<Nothing>
	{
		public NothingTestResponder(ITestResponder<T> responder)
		: base(() => new NothingOperationState<ITestOperationState<T>, ITestOperationState<Nothing>>(
			responder.CreateState(),
			TestOperationState.AsNothingOperationState))
		{
		}
	}
}
