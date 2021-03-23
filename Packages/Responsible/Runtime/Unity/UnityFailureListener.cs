using System;
using UnityEngine;

namespace Responsible.Unity
{
	/// <summary>
	/// Provider default <see cref="IFailureListener"/> functionality for Unity, by logging failures.
	/// Unity doesn't handle exceptions in coroutines very well in tests
	/// (one of the motivations to write Responsible), so we also log failures to ensure they are visible.
	/// </summary>
	public class UnityFailureListener : IFailureListener
	{
		/// <summary>
		/// Will log the failure message as a warning, if it was caused by an unhandled exception,
		/// or as an error, if it was caused by any other error.
		/// </summary>
		/// <inheritdoc cref="IFailureListener.OperationFailed"/>
		public void OperationFailed(Exception exception, string failureMessage)
		{
			if (exception is UnhandledLogMessageException)
			{
				Debug.LogWarning(failureMessage);
			}
			else
			{
				Debug.LogError(failureMessage);
			}
		}
	}
}
