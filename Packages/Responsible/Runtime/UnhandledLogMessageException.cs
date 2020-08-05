using System;

namespace Responsible
{
	public class UnhandledLogMessageException : Exception
	{
		public UnhandledLogMessageException(string condition)
			: base($"Unhandled log message: {condition}")
		{
		}
	}
}