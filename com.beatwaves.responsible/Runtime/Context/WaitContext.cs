using System;

namespace Responsible.Context
{
	internal class WaitContext
	{
		private readonly ITestScheduler scheduler;
		private readonly DateTimeOffset startTime;
		private readonly int startFrame;

		internal string ElapsedString =>
			$"{this.ElapsedTime.TotalSeconds:0.00} s and {this.ElapsedFrames} frames";

		private TimeSpan ElapsedTime => this.scheduler.TimeNow - this.startTime;
		private int ElapsedFrames => this.scheduler.FrameNow - this.startFrame;

		internal WaitContext(ITestScheduler scheduler)
		{
			this.scheduler = scheduler;
			this.startTime = scheduler.TimeNow;
			this.startFrame = scheduler.FrameNow;
		}
	}
}
