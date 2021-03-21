using System;

namespace Responsible.NoRx.Context
{
	internal class WaitContext
	{
		private readonly ITimeProvider timeProvider;
		private readonly DateTimeOffset startTime;
		private readonly int startFrame;

		internal string ElapsedString =>
			$"{this.ElapsedTime.TotalSeconds:0.00} s and {this.ElapsedFrames} frames";

		private TimeSpan ElapsedTime => this.timeProvider.TimeNow - this.startTime;
		private int ElapsedFrames => this.timeProvider.FrameNow - this.startFrame;

		internal WaitContext(ITimeProvider timeProvider)
		{
			this.timeProvider = timeProvider;
			this.startTime = timeProvider.TimeNow;
			this.startFrame = timeProvider.FrameNow;
		}
	}
}
