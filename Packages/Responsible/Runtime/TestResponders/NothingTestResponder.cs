using Responsible.State;

namespace Responsible.TestResponders
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
