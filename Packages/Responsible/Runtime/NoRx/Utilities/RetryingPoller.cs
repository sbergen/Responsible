using System;
using System.Collections.Generic;

namespace Responsible.NoRx.Utilities
{
	public class RetryingPoller
	{
		private readonly List<Action> callbacks = new List<Action>();
		private bool callbacksChanged;

		public IDisposable RegisterPollCallback(Action action)
		{
			void RemoveCallback()
			{
				this.callbacksChanged = true;
				this.callbacks.Remove(action);
			}

			this.callbacksChanged = true;
			this.callbacks.Add(action);
			return Disposable.Create(RemoveCallback);
		}

		public void Poll()
		{
			bool PollOnce()
			{
				this.callbacksChanged = false;
				foreach (var callback in this.callbacks)
				{
					callback();

					if (this.callbacksChanged)
					{
						return true;
					}
				}

				return false;
			}

			while (true)
			{
				if (!PollOnce())
				{
					break;
				}
			}
		}
	}
}
