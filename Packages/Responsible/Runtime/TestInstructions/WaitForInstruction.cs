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
			private readonly SourceContext sourceContext;

			public State(TimeSpan waitTime, SourceContext sourceContext)
			{
				this.waitTime = waitTime;
				this.sourceContext = sourceContext;
			}

			protected override IObservable<Unit> ExecuteInner(RunContext runContext)
				=> Observable.Timer(this.waitTime, runContext.Executor.Scheduler).AsUnitObservable();

			public override void BuildFailureContext(StateStringBuilder builder) =>
				builder.AddInstruction(this, $"WAIT FOR {this.waitTime:g}", this.sourceContext);
		}
	}
}