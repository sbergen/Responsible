using System;

namespace Responsible
{
	/// <summary>
	/// Abstraction for polling and time-based operations.
	/// All of such operations function based on registering callbacks to an instance of this interface.
	/// See also <see cref="Utilities.RetryingPoller"/> for an implementation you may use for poll callbacks,
	/// and <see cref="Unity.UnityTimeProvider"/> for a default Unity implementation.
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
		/// Returns the current time.
		/// The absolute value does not matter, as long as time progresses monotonically.
		/// Used for timeouts, and for providing state details for operations.
		/// </summary>
		/// <value>The current time.</value>
		/// <remarks>
		/// This exists mostly so that it can be mocked in internal Responsible tests.
		/// </remarks>
		DateTimeOffset TimeNow { get; }

		/// <summary>
		/// Registers a poll callback to be called at least once per frame.
		/// See <see cref="Utilities.RetryingPoller"/> for the suggested default strategy.
		/// All time and frame based operations run from these callbacks.
		/// </summary>
		/// <param name="action">Action to call at least once on every frame.</param>
		/// <returns>A disposable instance, which must unregister the callback when disposed.</returns>
		IDisposable RegisterPollCallback(Action action);
	}
}
