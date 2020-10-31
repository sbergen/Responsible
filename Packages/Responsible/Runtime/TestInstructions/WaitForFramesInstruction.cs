using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class WaitForFramesInstruction : TestInstructionBase<Unit>
	{
		public WaitForFramesInstruction(int frames, SourceContext sourceContext)
			: base(() => new State(frames, sourceContext))
		{
		}

		private class State : TestOperationState<Unit>
		{
			private readonly int frames;

			public State(int frames, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.frames = frames;
			}

			protected override IObservable<Unit> ExecuteInner(RunContext runContext)
				=> Observable.TimerFrame(this.frames).AsUnitObservable();

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddInstruction(this, $"WAIT FOR {this.frames} FRAME(S)");
		}
	}
}