using System;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class ExceptionMessageTests : ResponsibleTestBase
	{
		[Test]
		public void ExceptionMessage_IncludesAllDetails()
		{
			var task = Do("Throw", () => throw new Exception("An exception"))
				.ToTask(this.Executor);
			var failureException = GetFailureException(task);
			StateAssert
				.StringContainsInOrder(failureException.Message)
				.Details("failed")
				.Details("Throw")
				.Details("Error:")
				.Details("An exception");
		}
	}
}
