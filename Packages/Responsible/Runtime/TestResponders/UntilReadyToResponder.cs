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
		: base(() => new State(respondTo, untilReady, sourceContext))
		{
		}

		private class State : OperationState<IOperationState<T>>
		{
			private readonly IOperationState<IOperationState<Unit>> respondTo;
			private readonly IOperationState<IOperationState<T>> untilReady;

			public State(IOptionalTestResponder respondTo, ITestResponder<T> untilReady, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.respondTo = respondTo.CreateState();
				this.untilReady = untilReady.CreateState();
			}

			protected override IObservable<IOperationState<T>> ExecuteInner(RunContext runContext) => Observable.Defer(() =>
			{
				var replayedUntilResponse = new AsyncSubject<IOperationState<T>>();
				var waitSubscription = this.untilReady.Execute(runContext).Subscribe(replayedUntilResponse);

				return this.respondTo
					.Execute(runContext)
					// Keep waiting for condition even though the responder stream completes
					.Concat(Observable.Never<IOperationState<Unit>>())
					.TakeUntil(replayedUntilResponse)
					.Select(state => state.Execute(runContext))
					.Concat() // Execute the responder instructions
					.SelectMany(_ => Observable.Empty<IOperationState<T>>()) // Throw away responder results
					.Concat(replayedUntilResponse)
					.Finally(() =>
					{
						waitSubscription.Dispose();
						replayedUntilResponse.Dispose();
					});
			});

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddUntilResponder(
					"RESPOND TO",
					this.respondTo,
					"UNTIL READY TO",
					this.untilReady);
		}
	}
}