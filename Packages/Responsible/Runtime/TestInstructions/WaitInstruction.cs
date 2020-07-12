using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class WaitInstruction<T> : TestInstructionBase<T>
	{
		public WaitInstruction(
			ITestWaitCondition<T> condition,
			TimeSpan timeout,
			SourceContext sourceContext)
			: base(() => new State(condition, timeout, sourceContext))
		{
		}
		
		private class State : OperationState<T>
		{
			private readonly IOperationState<T> condition;
			private readonly TimeSpan timeout;

			public State(ITestWaitCondition<T> condition, TimeSpan timeout, SourceContext sourceContext)
			{
				this.condition = condition.CreateState();
				this.timeout = timeout;
				this.SourceContext = sourceContext;
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext) => this.condition
				.Execute(runContext)
				.Timeout(this.timeout, runContext.Scheduler);

			public override void BuildFailureContext(StateStringBuilder builder) =>
				builder.AddExpectWithin(this.timeout, this.condition);
		}
	}
}