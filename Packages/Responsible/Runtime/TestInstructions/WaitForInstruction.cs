using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class WaitForInstruction : TestInstructionBase<Unit>
	{
		public WaitForInstruction(TimeSpan waitTime, SourceContext sourceContext)
		: base(() => new State(waitTime, sourceContext))
		{
		}

		private class State : TestOperationState<Unit>
		{
			private readonly TimeSpan waitTime;

			public State(TimeSpan waitTime, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.waitTime = waitTime;
			}

			protected override IObservable<Unit> ExecuteInner(RunContext runContext)
				=> Observable.Timer(this.waitTime, runContext.Scheduler).AsUnitObservable();

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddInstruction(this, $"WAIT FOR {this.waitTime:g}");
		}
	}
}