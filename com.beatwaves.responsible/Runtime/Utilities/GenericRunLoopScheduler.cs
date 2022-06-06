using System;

namespace Responsible.Utilities
{
	/// <summary>
	/// Generic version of a run-loop scheduler, which does not simulate time.
	/// </summary>
	internal sealed class GenericRunLoopScheduler : RunLoopScheduler<object>
	{
		protected override void Tick(Action<object> tick) => tick(Unit.Instance);

		public override DateTimeOffset TimeNow => DateTimeOffset.Now;
	}
}
