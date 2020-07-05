using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class MultipleResponderWait<T> : ITestWaitCondition<T>
	{
		private readonly IOptionalTestResponder responder;
		private readonly ITestWaitCondition<T> condition;

		private AsyncSubject<Unit> currentInstruction;

		public void BuildDescription(ContextStringBuilder builder)
		{
			builder.Add("RESPOND TO", this.responder);
			builder.Add("UNTIL", this.condition);
		}

		public void BuildFailureContext(ContextStringBuilder builder) => this.BuildDescription(builder);

		public IObservable<T> WaitForResult(RunContext runContext, WaitContext waitContext) => Observable.Defer(() =>
		{
			var replayedResult = this.condition
				.WaitForResult(runContext, waitContext)
				.Replay(1)
				.RefCount();

			return this.responder
				.Instructions(runContext, waitContext)
				// Keep waiting for condition even though the instruction stream completes
				.Concat(Observable.Never<IObservable<Unit>>())
				.TakeUntil(replayedResult)
				.Concat() // Execute the instructions
				.SelectMany(_ => Observable.Empty<T>()) // Throw away instruction results
				.Concat(replayedResult);
		});

		public MultipleResponderWait(
			IOptionalTestResponder responder,
			ITestWaitCondition<T> condition)
		{
			this.responder = responder;
			this.condition = condition;
		}
	}
}