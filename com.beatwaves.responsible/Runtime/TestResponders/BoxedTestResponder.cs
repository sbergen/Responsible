using Responsible.State;

namespace Responsible.TestResponders
{
	internal class BoxedTestResponder<T> : TestResponderBase<object>
		where T : struct
	{
		public BoxedTestResponder(ITestResponder<T> responder)
			: base(() => new BoxedOperationState<ITestOperationState<T>, ITestOperationState<object>>(
				responder.CreateState(),
				TestOperationState.BoxResult!))
		{
		}
	}
}
