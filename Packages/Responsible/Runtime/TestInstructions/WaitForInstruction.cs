using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class WaitForInstruction : ITestInstruction<Unit>
	{
		private readonly TimeSpan waitTime;

		public IObservable<Unit> Run(RunContext runContext)
			=> Observable.Timer(this.waitTime, runContext.Executor.Scheduler).AsUnitObservable();

		public WaitForInstruction(TimeSpan waitTime)
		{
			this.waitTime = waitTime;
		}

		public void BuildDescription(ContextStringBuilder builder) =>
			builder.Add($"WAIT FOR {this.waitTime:g}");

		public void BuildFailureContext(ContextStringBuilder builder) => this.BuildDescription(builder);
	}
}