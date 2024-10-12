using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;

namespace Responsible.TestInstructions
{
	internal class ExpectConditionInstruction<T> : TestInstructionBase<T>
	{
		public ExpectConditionInstruction(
			ITestWaitCondition<T> condition,
			TimeSpan timeout,
			SourceContext sourceContext)
			: base(() => new State(condition, timeout, sourceContext))
		{
		}
		
		private class State : TestOperationState<T>
		{
			private readonly ITestOperationState<T> condition;
			private readonly TimeSpan timeout;

			public State(ITestWaitCondition<T> condition, TimeSpan timeout, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.condition = condition.CreateState();
				this.timeout = timeout;
			}

			protected override Task<T> ExecuteInner(RunContext runContext, CancellationToken cancellationToken) =>
				runContext.Scheduler.TimeoutAfter(
					this.timeout,
					cancellationToken,
					ct => this.condition.Execute(runContext, ct));

			protected override void BuildDescription(StateStringBuilder builder) =>
				builder.AddExpectWithin(this, this.timeout, this.condition);
		}
	}
}
