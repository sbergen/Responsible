namespace Responsible.State
{
	/// <summary>
	/// Represents the type of a test operation state transition.
	/// </summary>
	public enum TestOperationStateTransition
	{
		/// <summary>
		/// The operation was started.
		/// </summary>
		Started,

		/// <summary>
		/// The operation was completed successfully, with an error, or canceled.
		/// </summary>
		Finished,
	}
}
