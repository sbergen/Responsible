using System;

namespace Responsible.Unity
{
	/// <summary>
	/// Exception type which is used when an unhandled log message is detected during test operation execution.
	/// </summary>
	public class UnhandledLogMessageException : Exception
	{
		internal UnhandledLogMessageException(string condition, string stackTrace)
			: base($"Unhandled log message: {condition}\n---\n{stackTrace}")
		{
		}
	}
}
