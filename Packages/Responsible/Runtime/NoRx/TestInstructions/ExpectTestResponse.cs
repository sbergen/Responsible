using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;
using Responsible.NoRx.Utilities;

namespace Responsible.NoRx.TestInstructions
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

			protected override async Task<T> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
			{
				var instruction = await runContext.TimeProvider.TimeoutAfter(
					this.timeout,
					cancellationToken,
					ct => this.responder.Execute(runContext, ct));
				return await instruction.Execute(runContext, cancellationToken);
			}

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddExpectWithin(this, this.timeout, this.responder);
		}
	}
}