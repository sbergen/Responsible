using System;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

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
		public void RunLoopFrameCount_ContainsExpectedValues()
		{
			var frame = -1;
			var exception = Assert.Throws<TestFailureException>(() => Responsibly
				.WaitForCondition("frame", () => frame == 10)
				.ExpectWithinSeconds(1)
				.ContinueWith(Do("Throw", () => throw new Exception()))
				.RunAsLoop(() => ++frame));

			StateAssert.StringContainsInOrder(exception?.Message)
				.Completed("frame")
				.Details("≈ 10 frames");
		}

		[Test]
		public void RunAsLoop_ThrowsProperException_WhenTimedOut()
		{
			var exception = Assert.Throws<TestFailureException>(() => Responsibly
				.WaitForCondition("Never", () => false)
				.ExpectWithinSeconds(0)
				.RunAsLoop(() => { }));

			Assert.IsInstanceOf<TimeoutException>(exception?.InnerException);
			AssertMessageContainsOperationNameTag(exception);
		}

		[Test]
		public void RunAsLoop_ThrowsProperException_WhenInstructionThrows()
		{
			var exception = Assert.Throws<TestFailureException>(() => Responsibly
				.WaitForCondition(
					"Throw",
					() => throw new Exception(TestExceptionMessage))
				.ExpectWithinSeconds(1)
				.RunAsLoop(() => { }));

			Assert.AreEqual(TestExceptionMessage, exception?.InnerException?.Message);
			AssertMessageContainsOperationNameTag(exception);
		}

		[Test]
		public void RunAsLoop_ThrowsProperException_WhenRunLoopThrows()
		{
			var exception = Assert.Throws<TestFailureException>(() => Responsibly
				.WaitForCondition("Never", () => false)
				.ExpectWithinSeconds(1)
				.RunAsLoop(() => throw new Exception(TestExceptionMessage)));

			Assert.AreEqual(TestExceptionMessage, exception?.InnerException?.Message);
			// Can't (currently, easily) get the current instruction from this to include the instruction stack
		}

		private static void AssertMessageContainsOperationNameTag(TestFailureException exception) =>
			StringAssert.Contains(
				$"[{nameof(TestInstruction.RunAsLoop)}]",
				exception.Message);
	}
}
