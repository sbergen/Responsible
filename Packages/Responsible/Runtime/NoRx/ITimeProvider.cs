using System;

namespace Responsible.NoRx
{
	public interface ITimeProvider
	{
		int FrameNow { get; }
		DateTimeOffset TimeNow { get; }
		IDisposable RegisterPollCallback(Action action);
	}
}
