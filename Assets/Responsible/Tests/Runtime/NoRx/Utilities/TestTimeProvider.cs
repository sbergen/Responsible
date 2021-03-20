using System;
using Responsible.NoRx;
using Responsible.NoRx.Utilities;

namespace Responsible.Tests.Runtime.NoRx.Utilities
{
	public class TestTimeProvider : ITimeProvider
	{
		private RetryingPoller poller = new RetryingPoller();

		public DateTimeOffset TimeNow { get; private set; } = DateTimeOffset.Now;

		public IDisposable RegisterPollCallback(Action action) => this.poller.RegisterPollCallback(action);

		public void AdvanceFrame(TimeSpan time)
		{
			this.TimeNow += time;
			this.poller.Poll();
		}
	}
}