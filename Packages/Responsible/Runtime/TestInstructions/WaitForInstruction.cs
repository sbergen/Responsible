using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;

namespace Responsible.TestInstructions
{
	internal class WaitForInstruction : TestInstructionBase<Nothing>
	{
		public WaitForInstruction(TimeSpan waitTime, SourceContext sourceContext)
		: base(() => new State(waitTime, sourceContext))
		{
		}

		private class State : TestOperationState<Nothing>
		{
			private readonly TimeSpan waitTime;

			public State(TimeSpan waitTime, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.waitTime = waitTime;
			}

			protected override Task<Nothing> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				var deadline = runContext.TimeProvider.TimeNow + this.waitTime;
				return runContext.TimeProvider.PollForCondition(
					() => Nothing.Default,
					_ => runContext.TimeProvider.TimeNow >= deadline,
					cancellationToken);
			}

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddInstruction(this, $"WAIT FOR {this.waitTime:g}");
		}
	}
}
