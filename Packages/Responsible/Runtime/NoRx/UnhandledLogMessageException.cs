using System;

namespace Responsible.NoRx
{
	/// <summary>
	/// Exception type which is used when an unhandled log message is detected during test operation execution.
	/// </summary>
	public class UnhandledLogMessageException : Exception
	{
		internal UnhandledLogMessageException(string condition)
			: base($"Unhandled log message: {condition}")
		{
		}
	}
}
