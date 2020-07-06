using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Responsible.Context
{
	public class WaitContext : IDisposable
	{
		private readonly List<(ITestOperationContext, string)> completedWaits =
			new List<(ITestOperationContext, string)>();

		private readonly List<(ITestOperationContext from, ITestOperationContext to)> relations =
			new List<(ITestOperationContext, ITestOperationContext)>();

		private readonly DateTimeOffset startTime;
		private readonly IScheduler scheduler;
		private readonly IDisposable frameSubscription;

		private int frameCount;

		internal string ElapsedTime =>
			$"{(this.scheduler.Now - this.startTime).TotalSeconds:0.00}s and {this.frameCount} frames";

		internal IEnumerable<(ITestOperationContext context, string elapsed)> CompletedWaits => this.completedWaits;

		internal IEnumerable<ITestOperationContext> RelatedContexts(ITestOperationContext context) =>
			this.relations.Where(r => r.from == context).Select(r => r.to);

		public void MarkAsCompleted(ITestOperationContext context) =>
			this.completedWaits.Add((context, this.ElapsedTime));

		public void AddRelation(ITestOperationContext from, ITestOperationContext to) =>
			this.relations.Add((from, to));

		internal WaitContext(IScheduler scheduler, IObservable<Unit> frameObservable)
		{
			this.startTime = scheduler.Now;
			this.scheduler = scheduler;
			this.frameSubscription = frameObservable.Subscribe(_ => ++this.frameCount);
		}

		public void Dispose() => this.frameSubscription.Dispose();
	}
}