using System;

namespace Responsible.NoRx.Context
{
	internal class WaitContext : IDisposable
	{
		private readonly ITimeProvider timeProvider;
		private readonly DateTimeOffset startTime;
		private readonly IDisposable frameCountSubscription;

		private int frameCount;

		internal string ElapsedTime =>
			$"{(this.timeProvider.TimeNow - this.startTime).TotalSeconds:0.00} s and {this.frameCount} frames";

		internal WaitContext(ITimeProvider timeProvider)
		{
			this.timeProvider = timeProvider;
			this.startTime = timeProvider.TimeNow;
			this.frameCountSubscription = timeProvider.RegisterPollCallback(() => ++this.frameCount);
		}

		public void Dispose() => this.frameCountSubscription.Dispose();
	}
}
