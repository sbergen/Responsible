using System;

namespace Responsible
{
	public class TestFailureException : Exception
	{
		public TestFailureException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
