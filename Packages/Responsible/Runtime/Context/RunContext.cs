using System;
using UniRx;

namespace Responsible.Context
{
	public class RunContext : ContextBase
	{
		// This is private to enforce appending of current context!
		private readonly SourceContext sourceContext;

		internal readonly TestInstructionExecutor Executor;

		internal IObservable<Unit> PollObservable => this.Executor.PollObservable;

		internal WaitContext MakeWaitContext() => new WaitContext(this.Executor.Scheduler, this.PollObservable);

		internal SourceContext SourceContext(SourceContext current) => this.sourceContext.Append(current);

		internal RunContext(TestInstructionExecutor executor, SourceContext sourceContext)
		{
			this.Executor = executor;
			this.sourceContext = sourceContext;
		}
	}
}