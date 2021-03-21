using System;

namespace Responsible
{
	public interface ITimeProvider
	{
		int FrameNow { get; }
		DateTimeOffset TimeNow { get; }
		IDisposable RegisterPollCallback(Action action);
	}
}
