using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Responsible.Context
{
	public class WaitContext
	{
		private readonly List<(ITestOperationContext, string)> completedWaits =
			new List<(ITestOperationContext, string)>();

		private readonly DateTimeOffset startTime;
		private readonly int startFrame;
		private readonly IScheduler scheduler;

		internal string ElapsedTime =>
			$"{(this.scheduler.Now - this.startTime).TotalSeconds:0.00}s and {Time.frameCount - this.startFrame} frames";

		internal IEnumerable<(ITestOperationContext context, string elapsed)> CompletedWaits => this.completedWaits;

		public void MarkAsCompleted(ITestOperationContext context) =>
			this.completedWaits.Add((context, this.ElapsedTime));

		internal WaitContext(IScheduler scheduler)
		{
			this.startTime = scheduler.Now;
			this.startFrame = Time.frameCount;
			this.scheduler = scheduler;
		}
	}
}