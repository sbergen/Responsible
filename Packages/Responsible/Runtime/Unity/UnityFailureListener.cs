using System;
using UnityEngine;

namespace Responsible.Unity
{
	public class UnityFailureListener : IFailureListener
	{
		public void OperationFailed(Exception exception, string failureMessage)
		{
			// The Unity test runner can swallow exceptions, so both log an error and throw an exception.
			// For already logged errors, log this as a warning, as it contains extra context.
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
