using System;

namespace Responsible
{
	/// <summary>
	/// An exception, which indicates a failed test operation.
	/// <see cref="Exception.Message"/> includes the full details of the failure,
	/// and <see cref="Exception.InnerException"/> contains the exception that caused the failure.
	/// </summary>
	public class TestFailureException : Exception
	{
		internal TestFailureException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
