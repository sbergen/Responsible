using System;
using System.Collections.Generic;
using UniRx;

namespace Responsible.Context
{
	public class WaitContext : ContextBase, IDisposable
	{
		private readonly List<(ITestOperationContext, string)> completedWaits =
			new List<(ITestOperationContext, string)>();

		private readonly Subject<Unit> pollSubject = new Subject<Unit>();
		private readonly DateTimeOffset startTime;
		private readonly IScheduler scheduler;
		private readonly IDisposable frameSubscription;

		private int frameCount;
		private bool anyWaitsCompleted;

		internal IObservable<Unit> PollObservable => this.pollSubject;

		internal string ElapsedTime =>
			$"{(this.scheduler.Now - this.startTime).TotalSeconds:0.00}s and {this.frameCount} frames";

		internal IEnumerable<(ITestOperationContext context, string elapsed)> CompletedWaits => this.completedWaits;

		public void MarkAsCompleted(ITestOperationContext context)
		{
			this.completedWaits.Add((context, this.ElapsedTime));
			this.anyWaitsCompleted = true;
		}

		internal WaitContext(IScheduler scheduler, IObservable<Unit> frameObservable)
		{
			this.startTime = scheduler.Now;
			this.scheduler = scheduler;
			this.frameSubscription = frameObservable.Subscribe(_ =>
			{
				++this.frameCount;

				do
				{
					this.anyWaitsCompleted = false;
					this.pollSubject.OnNext(Unit.Default);
				} while (this.anyWaitsCompleted);
			});
		}

		public void Dispose() => this.frameSubscription.Dispose();
	}
}