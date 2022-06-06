using System;

namespace Responsible.Utilities
{
	internal class SimulatedUpdateLoopScheduler : RunLoopScheduler<TimeSpan>
	{
		private readonly TimeSpan frameDuration;
		private DateTimeOffset timeNow = DateTimeOffset.Now;

		public SimulatedUpdateLoopScheduler(double framesPerSecond) =>
			// On Unity (and older .NET), FromSeconds discards sub-millisecond information
			this.frameDuration = TimeSpan.FromTicks((int)Math.Round(TimeSpan.TicksPerSecond / framesPerSecond));

		protected override void Tick(Action<TimeSpan> tick)
		{
			tick(this.frameDuration);
			this.timeNow += this.frameDuration;
		}

		public override DateTimeOffset TimeNow => this.timeNow;
	}
}
