using System;
using Responsible.Context;
using UniRx;

namespace Responsible.State
{
	internal class UntilResponderState<T> : OperationState<T>
	{
		private readonly string untilDescription;
		private readonly IOperationState<IOperationState<Unit>> respondTo;
		private readonly IOperationState<T> until;

		public UntilResponderState(
			string untilDescription,
			IOperationState<IOperationState<Unit>> respondTo,
			IOperationState<T> until,
			SourceContext sourceContext)
			: base(sourceContext)
		{
			this.untilDescription = untilDescription;
			this.respondTo = respondTo;
			this.until = until;
		}

		protected override IObservable<T> ExecuteInner(RunContext runContext) => Observable.Defer(() =>
		{
			var replayedResult = new AsyncSubject<T>();
			var resultSubscription = this.until.Execute(runContext).Subscribe(replayedResult);

			return this.respondTo
				.Execute(runContext)
				// Keep waiting for condition even though the instruction stream completes
				.Concat(Observable.Never<IOperationState<Unit>>())
				.TakeUntil(replayedResult)
				.Select(state => state.Execute(runContext))
				.Concat() // Execute the instructions
				.SelectMany(_ => Observable.Empty<T>()) // Throw away instruction results
				.Concat(replayedResult)
				.Finally(() =>
				{
					resultSubscription.Dispose();
					replayedResult.Dispose();
				});
		});

		public override void BuildDescription(StateStringBuilder builder) =>
			builder.AddUntilResponder(
				"RESPOND TO",
				this.respondTo,
				this.untilDescription,
				this.until);
	}
}