namespace Responsible.NoRx.State
{
	/// <summary>
	/// Represents a notification for test operation state changes.
	/// See <see cref="TestInstructionExecutor.StateNotifications"/>.
	/// </summary>
	/// <remarks>
	/// This is mostly intended to be used with the Editor tools that ship with Responsible,
	/// but could also be used to build your own tooling.
	/// </remarks>
	public abstract class TestOperationStateNotification
	{
		/// <summary>
		/// State of the operation.
		/// Can be converted to a string representing the full state of the operation using <c>ToString()</c>.
		/// </summary>
		/// <value>State of the operation.</value>
		public readonly ITestOperationState State;

		private TestOperationStateNotification(ITestOperationState state)
		{
			this.State = state;
		}

		/// <summary>
		/// Notification for an operation that has been started.
		/// </summary>
		public class Started : TestOperationStateNotification
		{
			/// <summary>
			/// Intended for internal use only, public for tests.
			/// </summary>
			/// <param name="state">State to build the notification from.</param>
			public Started(ITestOperationState state)
				: base(state)
			{
			}
		}

		/// <summary>
		/// Notification for an operation that has been finished,
		/// either when canceled, completed, or on failure.
		/// </summary>
		public class Finished : TestOperationStateNotification
		{
			/// <summary>
			/// Intended for internal use only, public for tests.
			/// </summary>
			/// <param name="state">State to build the notification from.</param>
			public Finished(ITestOperationState state)
				: base(state)
			{
			}
		}
	}
}
