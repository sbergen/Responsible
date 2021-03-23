using System;

namespace Responsible
{
	/// <summary>
	/// Interface used for getting notified when test instructions fail.
	/// </summary>
	public interface IFailureListener
	{
		/// <summary>
		/// Called right before a test instruction fails.
		/// </summary>
		/// <param name="exception">Exception that caused the failure.</param>
		/// <param name="failureMessage">
		/// Message detailing the failure, including the state of the operation.
		/// </param>
		void OperationFailed(Exception exception, string failureMessage);
	}
}
