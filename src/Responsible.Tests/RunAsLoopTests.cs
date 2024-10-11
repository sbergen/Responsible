using System;
using System.Linq;
using System.Threading.Tasks;
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
		public async Task RunAsSimulatedUpdateLoop_UsesSimulatedFrameDurationForTime()
		{
			var frameCount = 0;
			TimeSpan frameDuration = default;

			_ = await Responsibly
				.WaitForSeconds(1)
				.RunAsSimulatedUpdateLoop(
					framesPerSecond: 60,
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
		public async Task RunAsLoop_CompletesWithExpectedValue()
		{
			var frame = 0;
			var result = await Responsibly
				.WaitForConditionOn(
					"Frame to be 10",
					() => frame,
					f => f == 10)
				.ExpectWithinSeconds(1)
				.RunAsLoop(() => ++frame);

			result.Should().Be(10);
		}

		[Test]
		public async Task RunLoopFrameCount_ContainsExpectedValues()
		{
			var frame = -1;
			Func<Task> run = async () => await Responsibly
				.WaitForCondition("frame", () => frame == 10)
				.ExpectWithinSeconds(1)
				.ContinueWith(Do("Throw", () => throw new Exception()))
				.RunAsLoop(() => ++frame);
			var exception = (await run.Should().ThrowAsync<TestFailureException>())
				.Subject.Single();

			StateAssert.StringContainsInOrder(exception.Message)
				.Completed("frame")
				.Details("≈ 10 frames");
		}

		[Test]
		public async Task RunAsLoop_ThrowsProperException_WhenTimedOut()
		{
			Func<Task> run = async () => await Responsibly
				.WaitForCondition("Never", () => false)
				.ExpectWithinSeconds(0)
				.RunAsLoop(() => { });
			var exception = (await run.Should().ThrowAsync<TestFailureException>())
				.Subject.Single();

			exception?.InnerException.Should().BeOfType<TimeoutException>();
			AssertMessageContainsOperationNameTag(exception);
		}

		[Test]
		public async Task RunAsLoop_ThrowsProperException_WhenInstructionThrows()
		{
			Func<Task> run = async () => await Responsibly
				.WaitForCondition(
					"Throw",
					() => throw new Exception(TestExceptionMessage))
				.ExpectWithinSeconds(1)
				.RunAsLoop(() => { });
			var exception = (await run.Should().ThrowAsync<TestFailureException>())
				.Subject.Single();

			exception?.InnerException?.Message.Should().Be(TestExceptionMessage);
			AssertMessageContainsOperationNameTag(exception);
		}

		[Test]
		public async Task RunAsLoop_ThrowsProperException_WhenRunLoopThrows()
		{
			Func<Task> run = async () => await Responsibly
				.WaitForCondition("Never", () => false)
				.ExpectWithinSeconds(1)
				.RunAsLoop(() => throw new Exception(TestExceptionMessage));
			var exception = (await run.Should().ThrowAsync<TestFailureException>())
				.Subject.Single();

			exception?.InnerException?.Message.Should().Be(TestExceptionMessage);
			// Can't (currently, easily) get the current instruction from this to include the instruction stack
		}

		private static void AssertMessageContainsOperationNameTag(TestFailureException exception) =>
			exception.Message.Should().Contain($"[{nameof(TestInstruction.RunAsLoop)}]");
	}
}
