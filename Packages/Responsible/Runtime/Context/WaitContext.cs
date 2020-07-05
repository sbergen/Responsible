using System;
using System.Collections.Generic;
using UnityEngine;

namespace Responsible.Context
{
	public class WaitContext
	{
		private readonly List<(ITestOperationContext, string)> completedWaits =
			new List<(ITestOperationContext, string)>();

		private readonly DateTime startTime;
		private readonly int startFrame;

		internal string ElapsedTime =>
			$"{(DateTime.Now - this.startTime).TotalSeconds:0.00}s and {Time.frameCount - this.startFrame} frames";

		internal IEnumerable<(ITestOperationContext context, string elapsed)> CompletedWaits => this.completedWaits;

		public void MarkAsCompleted(ITestOperationContext context) =>
			this.completedWaits.Add((context, this.ElapsedTime));

		internal WaitContext()
		{
			this.startTime = DateTime.Now;
			this.startFrame = Time.frameCount;
		}
	}
}