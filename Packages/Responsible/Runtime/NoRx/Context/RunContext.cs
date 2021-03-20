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
		internal readonly ITimeProvider TimeProvider;
		internal readonly SourceContext SourceContext;

		internal WaitContext MakeWaitContext() => new WaitContext(this.TimeProvider);

		internal RunContext MakeNested(SourceContext sourceContext)
			=> new RunContext(this.SourceContext.Append(sourceContext), this.TimeProvider);

		internal RunContext(SourceContext sourceContext, ITimeProvider timeProvider)
		{
			this.SourceContext = sourceContext;
			this.TimeProvider = timeProvider;
		}
	}
}
