using System;
using Responsible.Context;
using Responsible.State;
using UniRx;
using UnityEngine;

namespace Responsible.TestWaitConditions
{
	internal class OptionalResponderWait<T> : TestWaitConditionBase<T>
	{
		public OptionalResponderWait(IOptionalTestResponder responder, ITestWaitCondition<T> condition)
			: base(() => new State(responder, condition))
		{
		}

		private class State : OperationState<T>
		{
			private readonly IOperationState<IOperationState<Unit>> responder;
			private readonly IOperationState<T> condition;

			public State(IOptionalTestResponder responder, ITestWaitCondition<T> condition)
			{
				this.responder = responder.CreateState();
				this.condition = condition.CreateState();
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext) => Observable.Defer(() =>
			{
				var replayedResult = new AsyncSubject<T>();
				var resultSubscription = this.condition.Execute(runContext).Subscribe(replayedResult);

				return this.responder
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

			public override void BuildFailureContext(StateStringBuilder builder) =>
				builder.AddOptionalResponder(
					"RESPOND TO",
					this.responder,
					"UNTIL",
					this.condition);
		}
	}
}