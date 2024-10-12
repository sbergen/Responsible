using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;

namespace Responsible.TestInstructions
{
	internal class WaitForInstruction : TestInstructionBase<object>
	{
		public WaitForInstruction(TimeSpan waitTime, SourceContext sourceContext)
		: base(() => new State(waitTime, sourceContext))
		{
		}

		private class State : TestOperationState<object>
		{
			private readonly TimeSpan waitTime;

			public State(TimeSpan waitTime, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.waitTime = waitTime;
			}

			protected override Task<object> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				var deadline = runContext.Scheduler.TimeNow + this.waitTime;
				return runContext.Scheduler.PollForCondition(
					() => Unit.Instance,
					_ => runContext.Scheduler.TimeNow >= deadline,
					cancellationToken);
			}

			protected override void BuildDescription(StateStringBuilder builder) =>
				builder.AddInstruction(this, $"WAIT FOR {this.waitTime:g}");
		}
	}
}
