using System;
using Responsible.Context;
using Responsible.State;
using Responsible.TestWaitConditions;

namespace Responsible.TestResponders
{
	internal class UntilReadyToResponder<T> : TestResponderBase<T>
	{
		public UntilReadyToResponder(IOptionalTestResponder respondTo, ITestResponder<T> untilReady)
		: base(() => new State(respondTo, untilReady))
		{
		}

		private class State : OperationState<IOperationState<T>>
		{
			private readonly IOperationState<IOperationState<T>> respondTo;
			private readonly IOperationState<IOperationState<T>> untilReady;

			public State(IOptionalTestResponder respondTo, ITestResponder<T> untilReady)
			{
				this.untilReady = untilReady.CreateState();
				this.respondTo = new OptionalResponderWait<IOperationState<T>>(
						respondTo,
						this.untilReady)
					.CreateState();
			}

			protected override IObservable<IOperationState<T>> ExecuteInner(RunContext runContext) =>
				this.respondTo.Execute(runContext);

			public override void BuildFailureContext(StateStringBuilder builder) =>
				builder.AddUntilResponder(
					"RESPONDING TO",
					this.respondTo,
					"UNTIL READY TO",
					this.untilReady);
		}
	}
}