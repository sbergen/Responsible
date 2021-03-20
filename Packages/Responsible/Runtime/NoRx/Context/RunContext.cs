using System;
using System.Threading;

namespace Responsible.NoRx.Context
{
	/// <summary>
	/// Represents the context in which a test operation is run from.
	/// Only for internal use.
	/// </summary>
	public readonly struct RunContext
	{
		private readonly ITimeProvider timeProvider;

		internal readonly SourceContext SourceContext;

		internal IDisposable RegisterPollCallback(Action action) => this.timeProvider.RegisterPollCallback(action);

		internal WaitContext MakeWaitContext() => new WaitContext(this.timeProvider);

		internal RunContext MakeNested(SourceContext sourceContext)
			=> new RunContext(this.SourceContext.Append(sourceContext), this.timeProvider);

		internal RunContext(SourceContext sourceContext, ITimeProvider timeProvider)
		{
			this.SourceContext = sourceContext;
			this.timeProvider = timeProvider;
		}
	}
}
