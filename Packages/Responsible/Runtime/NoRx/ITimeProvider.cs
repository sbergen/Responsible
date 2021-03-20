using System;

namespace Responsible.NoRx
{
	public interface ITimeProvider
	{
		DateTimeOffset TimeNow { get; }
		IDisposable RegisterPollCallback(Action action);
	}
}
