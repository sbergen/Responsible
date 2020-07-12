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

		private class State : OperationState<Unit>
		{
			private readonly TimeSpan waitTime;

			public State(TimeSpan waitTime, SourceContext sourceContext)
			{
				this.waitTime = waitTime;
				this.SourceContext = sourceContext;
			}

			protected override IObservable<Unit> ExecuteInner(RunContext runContext)
				=> Observable.Timer(this.waitTime, runContext.Scheduler).AsUnitObservable();

			public override void BuildFailureContext(StateStringBuilder builder) =>
				builder.AddInstruction(this, $"WAIT FOR {this.waitTime:g}");
		}
	}
}