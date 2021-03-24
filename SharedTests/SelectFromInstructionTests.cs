using System;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class SelectFromInstructionTests : ResponsibleTestBase
	{
		[Test]
		public void SelectFromInstruction_GetsApplied_WhenSuccessful()
		{
			var result = Return(2)
				.Select(val => val * 2)
				.ToTask(this.Executor)
				.AssertSynchronousResult();
			Assert.AreEqual(4, result);
		}

		[Test]
		public void SelectFromInstruction_PublishesCorrectError_WhenExceptionThrown()
		{
			var task = Return(2)
				.Select<int, int>(_ => throw new Exception("Fail!"))
				.ToTask(this.Executor);
			Assert.IsNotNull(GetFailureException(task));
		}

		[Test]
		public void SelectFromInstruction_ContainsFailureDetails_WhenFailed()
		{
			var task = Return(2)
				.Select<int, int>(_ => throw new Exception("Fail!"))
				.ToTask(this.Executor);

			var exception = GetFailureException(task);
			StringAssert.Contains(
				"[!] SELECT",
				exception.Message);
		}

		[Test]
		public void SelectFromInstruction_ContainsCorrectDetails_WhenInstructionFailed()
		{
			var task = DoAndReturn<int>("Throw", () => throw new Exception("Fail!"))
				.Select(i => i)
				.ToTask(this.Executor);

			var exception = GetFailureException(task);

			StringAssert.Contains(
				"[ ] SELECT",
				exception.Message,
				"Should not contain error for select");

			StringAssert.Contains(
				"[!] Throw",
				exception.Message,
				"Should contain error for instruction");
		}
	}
}
