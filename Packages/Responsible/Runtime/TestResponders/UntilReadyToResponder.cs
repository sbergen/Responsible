using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestResponders
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