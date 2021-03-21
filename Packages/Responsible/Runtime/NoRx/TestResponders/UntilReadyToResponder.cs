using Responsible.NoRx.Context;
using Responsible.NoRx.State;

namespace Responsible.NoRx.TestResponders
{
	internal class UntilReadyToResponder<T> : TestResponderBase<T>
	{
		public UntilReadyToResponder(
			IOptionalTestResponder respondTo,
			ITestResponder<T> untilReady,
			SourceContext sourceContext)
			: base(() => new UntilResponderState<ITestOperationState<T>>(
				"UNTIL READY TO",
				respondTo.CreateState(),
				untilReady.CreateState(),
				sourceContext))
		{
		}
	}
}
