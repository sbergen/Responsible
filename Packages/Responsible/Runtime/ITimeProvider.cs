using System;

namespace Responsible
{
	/// <summary>
	/// Abstraction for polling and time-based operations.
	/// All of such operations function based on registering callbacks to an instance of this interface.
	/// See also <see cref="Utilities.RetryingPoller"/> for an implementation you may use for poll callbacks.
	/// </summary>
	public interface ITimeProvider
	{
		/// <summary>
		/// Gets the frame count at this moment.
		/// The absolute value does not matter, as long as it is incremented periodically.
		/// Used for waiting for frames, and for providing state details for operations.
		/// </summary>
		/// <value>Current frame value.</value>
		int FrameNow { get; }

		/// <summary>
		/// Returns the current time. Is used in internal tests to mock time.
		/// </summary>
		/// <value>The current time.</value>
		DateTimeOffset TimeNow { get; }

		/// <summary>
		/// When called, will register a callback to be called on every frame.
		/// All time and frame based operations run from these callbacks.
		/// </summary>
		/// <param name="action">Action to call on every frame.</param>
		/// <returns>A disposable instance, which must unregister the callback when disposed.</returns>
		IDisposable RegisterPollCallback(Action action);
	}
}
