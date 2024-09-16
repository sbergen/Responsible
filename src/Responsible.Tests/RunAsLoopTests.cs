using System;
using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class RunAsLoopTests
	{
		private const string TestExceptionMessage = "Test exception";

		[Test]
		public void RunAsSimulatedUpdateLoop_UsesSimulatedFrameDurationForTime()
		{
			var frameCount = 0;
			TimeSpan frameDuration = default;

			_ = Responsibly
				.WaitForSeconds(1)
				.RunAsSimulatedUpdateLoop(
					60,
					duration =>
					{
						++frameCount;
						frameDuration = duration;
					});

			// On Unity (and older .NET), FromSeconds discards sub-millisecond information
			frameDuration.Should().Be(TimeSpan.FromTicks((int)Math.Round(TimeSpan.TicksPerSecond / 60.0)));
			frameCount.Should().Be(60);
		}

		[Test]
		public void RunAsLoop_CompletesWithExpectedValue()
		{
			var frame = 0;
			var result = Responsibly
				.WaitForPredicate(
					"Frame to be 10",
					() => frame,
					f => f == 10)
				.ExpectWithinSeconds(1)
				.RunAsLoop(() => ++frame);

			result.Should().Be(10);
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

			exception?.InnerException.Should().BeOfType<TimeoutException>();
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

			exception?.InnerException?.Message.Should().Be(TestExceptionMessage);
			AssertMessageContainsOperationNameTag(exception);
		}

		[Test]
		public void RunAsLoop_ThrowsProperException_WhenRunLoopThrows()
		{
			var exception = Assert.Throws<TestFailureException>(() => Responsibly
				.WaitForCondition("Never", () => false)
				.ExpectWithinSeconds(1)
				.RunAsLoop(() => throw new Exception(TestExceptionMessage)));

			exception?.InnerException?.Message.Should().Be(TestExceptionMessage);
			// Can't (currently, easily) get the current instruction from this to include the instruction stack
		}

		private static void AssertMessageContainsOperationNameTag(TestFailureException exception) =>
			exception.Message.Should().Contain($"[{nameof(TestInstruction.RunAsLoop)}]");
	}
}
