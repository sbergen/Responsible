using System;
using UniRx;

namespace Responsible.Context
{
	/// <summary>
	/// Represents the context in which a test operation is run from.
	/// Only for internal use.
	/// </summary>
	public readonly struct RunContext
	{
		internal readonly SourceContext SourceContext;
		internal readonly IScheduler Scheduler;
		internal readonly IObservable<Unit> PollObservable;

		internal WaitContext MakeWaitContext() => new WaitContext(this.Scheduler, this.PollObservable);

		internal RunContext MakeNested(SourceContext sourceContext)
			=> new RunContext(this.SourceContext.Append(sourceContext), this.Scheduler, this.PollObservable);

		internal RunContext(SourceContext sourceContext, IScheduler scheduler, IObservable<Unit> pollObservable)
		{
			this.SourceContext = sourceContext;
			this.Scheduler = scheduler;
			this.PollObservable = pollObservable;
		}
	}
}
