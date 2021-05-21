namespace Responsible.Context
{
	/// <summary>
	/// Represents the context in which a test operation is run from.
	/// Only for internal use.
	/// </summary>
	public readonly struct RunContext
	{
		internal readonly ITestScheduler Scheduler;
		internal readonly SourceContext SourceContext;

		internal WaitContext MakeWaitContext() => new WaitContext(this.Scheduler);

		internal RunContext MakeNested(SourceContext sourceContext)
			=> new RunContext(this.SourceContext.Push(sourceContext), this.Scheduler);

		internal RunContext(SourceContext sourceContext, ITestScheduler scheduler)
		{
			this.SourceContext = sourceContext;
			this.Scheduler = scheduler;
		}
	}
}
