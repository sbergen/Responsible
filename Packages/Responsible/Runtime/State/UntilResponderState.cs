using System;
using Responsible.Context;
using UniRx;

namespace Responsible.State
{
	internal class UntilResponderState<T> : TestOperationState<T>
	{
		private readonly string untilDescription;
		private readonly ITestOperationState<ITestOperationState<Unit>> respondTo;
		private readonly ITestOperationState<T> until;

		public UntilResponderState(
			string untilDescription,
			ITestOperationState<ITestOperationState<Unit>> respondTo,
			ITestOperationState<T> until,
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
				.Concat(Observable.Never<ITestOperationState<Unit>>())
				.TakeUntil(replayedResult)
				.Select(state => state.Execute(runContext))
				.Concat() // Execute the instructions
				.SelectMany(_ => Observable.Empty<T>()) // Throw away instruction results
				.Concat(replayedResult)
				.Finally(resultSubscription.Dispose);
		});

		public override void BuildDescription(StateStringBuilder builder) =>
			builder.AddUntilResponder(
				"RESPOND TO",
				this.respondTo,
				this.untilDescription,
				this.until);
	}
}