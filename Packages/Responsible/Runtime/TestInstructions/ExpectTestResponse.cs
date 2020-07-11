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

		private class State : OperationState<T>
		{
			private readonly IOperationState<IOperationState<T>> responder;
			private readonly TimeSpan timeout;
			private readonly SourceContext sourceContext;

			public State(ITestResponder<T> responder, TimeSpan timeout, SourceContext sourceContext)
			{
				this.responder = responder.CreateState();
				this.timeout = timeout;
				this.sourceContext = sourceContext;
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext) => this.responder
				.Execute(runContext)
				.Timeout(this.timeout, runContext.Executor.Scheduler)
				.ContinueWith(instruction => instruction.Execute(runContext));

			public override void BuildFailureContext(StateStringBuilder builder) =>
				builder.AddExpectWithin(this.timeout, this.responder, this.sourceContext);
		}
	}
}