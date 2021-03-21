using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;
using Responsible.NoRx.Utilities;

namespace Responsible.NoRx.TestInstructions
{
	internal class WaitForFramesInstruction : TestInstructionBase<Nothing>
	{
		public WaitForFramesInstruction(int frames, SourceContext sourceContext)
			: base(() => new State(frames, sourceContext))
		{
		}

		private class State : TestOperationState<Nothing>
		{
			private readonly int wholeFramesToWaitFor;
			private int passedFrames = -1; // Compensate for immediate poll

			public State(int wholeFramesToWaitFor, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.wholeFramesToWaitFor = wholeFramesToWaitFor;
			}

			protected override Task<Nothing> ExecuteInner(RunContext runContext, CancellationToken cancellationToken) =>
				runContext.TimeProvider.PollForCondition(
					() => Nothing.Default,
					_ => this.passedFrames++ >= this.wholeFramesToWaitFor,
					cancellationToken);

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddInstruction(this, $"WAIT FOR {this.wholeFramesToWaitFor} FRAME(S)");
		}
	}
}
