using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class ExpectTestResponse<T> : TestInstructionBase<T>
	{
		public ExpectTestResponse(
			ITestResponder<T> responder,
			TimeSpan timeout,
			SourceContext sourceContext)
		: base(() => new State(responder, timeout, sourceContext))
		{
		}

		private class State : TestOperationState<T>
		{
			private readonly ITestOperationState<ITestOperationState<T>> responder;
			private readonly TimeSpan timeout;

			public State(ITestResponder<T> responder, TimeSpan timeout, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.responder = responder.CreateState();
				this.timeout = timeout;
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext) => this.responder
				.Execute(runContext)
				.Timeout(this.timeout, runContext.Scheduler)
				.ContinueWith(instruction =>
					instruction.Execute(runContext));

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddExpectWithin(this.timeout, this.responder);
		}
	}
}