using System;
using Responsible.Utilities;

namespace Responsible.Tests.Utilities
{
	public class MockTestScheduler : ITestScheduler
	{
		private RetryingPoller poller = new RetryingPoller();

		public int FrameNow { get; private set; }
		public DateTimeOffset TimeNow { get; private set; } = DateTimeOffset.Now;

		public IDisposable RegisterPollCallback(Action action) => this.poller.RegisterPollCallback(action);

		public void AdvanceFrame(TimeSpan time)
		{
			++this.FrameNow;
			this.TimeNow += time;
			this.poller.Poll();
		}
	}
}
