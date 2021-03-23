using System;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class SelectFromWaitConditionTests : ResponsibleTestBase
	{
		[Test]
		public void SelectFromCondition_GetsApplied_WhenSuccessful()
		{
			var result = ImmediateTrue
				.Select(r => r ? 1 : 0)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor)
				.AssertSynchronousResult();

			Assert.AreEqual(1, result);
		}

		[Test]
		public void SelectFromCondition_PublishesError_WhenExceptionThrown()
		{
			var task = ImmediateTrue
				.Select<bool, int>(r => throw new Exception("Fail!"))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			Assert.IsNotNull(GetAssertionException(task));
		}

		[Test]
		public void SelectFromCondition_ContainsFailureDetails_WhenFailed()
		{
			var task = ImmediateTrue
				.Select<bool, int>(r => throw new Exception("Fail!"))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			var error = GetAssertionException(task);
			StringAssert.Contains(
				"[!] SELECT",
				error.Message);
		}

		[Test]
		public void SelectFromCondition_ContainsCorrectDetails_WhenConditionFailed()
		{
			var task = WaitForCondition("Throw", () => throw new Exception("Fail!"))
				.Select(r => r)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			var error = GetAssertionException(task);

			StringAssert.Contains(
				"[ ] SELECT",
				error.Message,
				"Should not contain error for Select");

			StringAssert.Contains(
				"[!] Throw",
				error.Message,
				"Should contain error for condition");
		}
	}
}
