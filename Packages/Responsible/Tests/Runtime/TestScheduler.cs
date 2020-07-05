using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UniRx;

namespace Responsible.Tests.Runtime
{
	/// <summary>
	/// Test scheduler assuming use from a single thread
	/// </summary>
	public class TestScheduler : IScheduler
	{
		private class ScheduledAction
		{
			public readonly Action Action;
			public readonly DateTimeOffset Time;

			public ScheduledAction(Action action, DateTimeOffset time)
			{
				this.Action = action;
				this.Time = time;
			}
		}

		private readonly List<ScheduledAction> scheduledActions = new List<ScheduledAction>();
		
		public IDisposable Schedule(Action action)
		{
			action();
			return Disposable.Empty;
		}

		public IDisposable Schedule(TimeSpan dueTime, Action action)
		{
			var time = this.Now + Scheduler.Normalize(dueTime);
			if (time <= this.Now)
			{
				action();
				return Disposable.Empty;
			}
			else
			{
				var scheduledAction = new ScheduledAction(action, time);
				this.scheduledActions.Add(scheduledAction);
				return Disposable.Create(() => this.scheduledActions.Remove(scheduledAction));
			}
		}

		public DateTimeOffset Now { get; private set; }

		public void AdvanceBy(TimeSpan offset)
		{
			var until = this.Now + offset;
			while (true)
			{
				var next = this.GetNextAction();
				if (next == null || next.Time > until)
				{
					break;
				}

				this.Now = next.Time;
				next.Action();
				this.scheduledActions.Remove(next);
			}

			this.Now = until;
		}

		// Not super optimized...
		[CanBeNull]
		private ScheduledAction GetNextAction() =>
			this.scheduledActions.OrderBy(action => action.Time).FirstOrDefault();

	}
}