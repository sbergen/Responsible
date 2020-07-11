using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class ExpectTestResponse<T> : ITestInstruction<T>
	{
		private readonly ITestResponder<T> responder;
		private readonly TimeSpan timeout;
		private readonly SourceContext sourceContext;

		public ExpectTestResponse(
			ITestResponder<T> responder,
			TimeSpan timeout,
			SourceContext sourceContext)
		{
			this.responder = responder;
			this.timeout = timeout;
			this.sourceContext = sourceContext;
		}

		public IObservable<T> Run(RunContext runContext)
			=> runContext.Executor
			.WaitFor(
				this.responder.InstructionWaitCondition,
				this.timeout,
				runContext.SourceContext(this.sourceContext))
			.ContinueWith(instruction => instruction.Run(runContext))
			.Do(_ => runContext.MarkAsCompleted(this));

		public void BuildDescription(ContextStringBuilder builder) => this.responder.BuildDescription(builder);

		public void BuildFailureContext(ContextStringBuilder builder)
		{
			builder.AddInstructionStatus(
				this,
				this.sourceContext,
				$"EXPECT WITHIN {this.timeout:g}",
				this.responder);
		}
	}
}