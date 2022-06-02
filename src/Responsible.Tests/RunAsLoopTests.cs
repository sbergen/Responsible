using System;
using NUnit.Framework;

namespace Responsible.Tests
{
	public class RunAsLoopTests
	{
		private const string TestExceptionMessage = "Test exception";

		[Test]
		public void RunAsLoop_CompletesWithExpectedValue()
		{
			var frame = 0;
			var result = Responsibly
				.WaitForConditionOn(
					"Frame to be 10",
					() => frame,
					f => f == 10)
				.ExpectWithinSeconds(1)
				.RunAsLoop(() => ++frame);

			Assert.AreEqual(10, result);
		}

		[Test]
		public void RunAsLoop_ThrowsTestFailureException_WhenTimedOut()
		{
			var exception = Assert.Throws<TestFailureException>(() => Responsibly
				.WaitForCondition("Never", () => false)
				.ExpectWithinSeconds(0)
				.RunAsLoop(() => { }));

			Assert.IsInstanceOf<TimeoutException>(exception?.InnerException);
		}

		[Test]
		public void RunAsLoop_ThrowsTestFailureException_WhenInstructionThrows()
		{
			var exception = Assert.Throws<TestFailureException>(() => Responsibly
				.WaitForCondition(
					"Throw",
					() => throw new Exception(TestExceptionMessage))
				.ExpectWithinSeconds(1)
				.RunAsLoop(() => { }));

			Assert.AreEqual(TestExceptionMessage, exception?.InnerException?.Message);
		}

		[Test]
		public void RunAsLoop_ThrowsTestFailureException_WhenRunLoopThrows()
		{
			var exception = Assert.Throws<TestFailureException>(() => Responsibly
				.WaitForCondition("Never", () => false)
				.ExpectWithinSeconds(1)
				.RunAsLoop(() => throw new Exception(TestExceptionMessage)));

			Assert.AreEqual(TestExceptionMessage, exception?.InnerException?.Message);
		}
	}
}
