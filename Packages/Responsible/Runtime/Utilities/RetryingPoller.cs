using System;

namespace Responsible.Utilities
{
	public class RetryingPoller
	{
		private readonly SafeIterationList<Action> callbacks = new SafeIterationList<Action>();

		public IDisposable RegisterPollCallback(Action action) =>
			this.callbacks.Add(action);

		public void Poll()
		{
			while (this.callbacks.ForEach(callback => callback()))
			{
				// Poll again if any changes
			}
		}
	}
}
