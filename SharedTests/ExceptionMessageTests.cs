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

			Console.WriteLine(failureException.Message);

			StateAssert
				.StringContainsInOrder(failureException.Message)
				.Details("Test operation execution failed")
				.Failed("Throw")
				.Details("Failed with:")
				.Details("An exception")
				.Details("Test operation stack:")
				.Details(@"\[Do\].*?\(at")
				.Details(@"\[ToTask\].*?\(at")
				.Details("Error:");
		}

		[Test]
		public void ExceptionMessage_DoesNotIncludeEmptyLines()
		{
			var task = Do("Throw", () => throw new Exception())
				.ToTask(this.Executor);
			var lines = GetFailureException(task).Message.Split("\n");
			CollectionAssert.Contains(lines, " ");
			CollectionAssert.DoesNotContain(lines, "");
		}
	}
}
