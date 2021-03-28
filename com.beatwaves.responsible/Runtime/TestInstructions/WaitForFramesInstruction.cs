using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;

namespace Responsible.TestInstructions
{
	internal class WaitForFramesInstruction : TestInstructionBase<object>
	{
		public WaitForFramesInstruction(int frames, SourceContext sourceContext)
			: base(() => new State(frames, sourceContext))
		{
		}

		private class State : TestOperationState<object>
		{
			private readonly int wholeFramesToWaitFor;

			public State(int wholeFramesToWaitFor, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.wholeFramesToWaitFor = wholeFramesToWaitFor;
			}

			protected override Task<object> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
			{
				var scheduler = runContext.Scheduler;
				var deadline = scheduler.FrameNow + this.wholeFramesToWaitFor;
				return runContext.Scheduler.PollForCondition(
					() => Unit.Instance,
					_ => scheduler.FrameNow > deadline,
					cancellationToken);
			}

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddInstruction(this, $"WAIT FOR {this.wholeFramesToWaitFor} FRAME(S)");
		}
	}
}
