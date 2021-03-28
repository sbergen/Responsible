using System;

namespace Responsible.Utilities
{
	/// <summary>
	/// Keeps a list of callbacks, and calls them repeatedly, until the list doesn't change during the calls.
	/// </summary>
	/// <remarks>
	/// May be used to build your own <see cref="TestInstructionExecutor"/>.
	/// The behavior provided by this class is intended for running tests as fast as possible,
	/// assuming polling is a relatively cheap operation to execute,
	/// and that executing some operations may complete other wait conditions.
	///
	/// The implementation focuses on simplicity (of code) over performance.
	/// It's unlikely to become a performance bottleneck, but if this does happen for you,
	/// please consider opening an issue or contributing code for a more performant poller.
	/// </remarks>
	public class RetryingPoller
	{
		private readonly SafeIterationList<Action> callbacks = new SafeIterationList<Action>();

		/// <summary>
		/// Registers a callback to be called at least once when <see cref="Poll"/> is called.
		/// </summary>
		/// <param name="action">Callback to be called</param>
		/// <returns>A disposable, which will remove this callback when disposed.</returns>
		public IDisposable RegisterPollCallback(Action action) =>
			this.callbacks.Add(action);

		/// <summary>
		/// Calls all the registered callbacks at least once, and keeps calling them
		/// until none of the callbacks end up unregistering old callbacks or registering new callbacks.
		/// </summary>
		public void Poll()
		{
			while (this.callbacks.ForEach(callback => callback()))
			{
				// Poll again if any changes
			}
		}
	}
}
