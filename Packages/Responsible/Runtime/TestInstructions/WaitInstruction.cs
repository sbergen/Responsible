using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class WaitInstruction<T> : ITestInstruction<T>
	{
		private readonly ITestWaitCondition<T> condition;
		private readonly TimeSpan timeout;
		private readonly SourceContext sourceContext;

		public WaitInstruction(
			ITestWaitCondition<T> condition,
			TimeSpan timeout,
			SourceContext sourceContext)
		{
			this.condition = condition;
			this.timeout = timeout;
			this.sourceContext = sourceContext;
		}

		public IObservable<T> Run(RunContext runContext)
			=> runContext.Executor
				.WaitFor(this.condition, this.timeout, runContext.SourceContext(this.sourceContext))
				.Do(_ => runContext.MarkAsCompleted(this));

		public void BuildDescription(ContextStringBuilder builder) => this.condition.BuildDescription(builder);

		public void BuildFailureContext(ContextStringBuilder builder)
			=> builder.AddInstructionStatus(
				this,
				this.sourceContext,
				$"EXPECT WITHIN {this.timeout:g}",
				this.condition);
	}
}