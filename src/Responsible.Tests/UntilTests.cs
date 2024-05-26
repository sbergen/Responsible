using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests
{
	public class UntilTests : ResponsibleTestBase
	{
		[Test]
		public void Until_CompletesResponder_WhenReadyBeforeCompletion()
		{
			var readyToReact = false;
			var startedToReact = false;
			var mayCompleteReaction = false;
			var reactionCompleted = false;
			var shouldContinue = false;

			var react = DoAndReturn("Set started to react", () => startedToReact = true)
				.ContinueWith(WaitForCondition("May complete", () => mayCompleteReaction)
					.ExpectWithinSeconds(1)
					.ContinueWith(DoAndReturn("Set reaction completed", () => reactionCompleted = true)));

			var respondUntil = WaitForCondition("Ready", () => readyToReact)
				.ThenRespondWith("React", react)
				.Optionally()
				.Until(WaitForCondition("Continue", () => shouldContinue))
				.ExpectWithinSeconds(1);

			var task = respondUntil.ToTask(this.Executor);

			readyToReact = true;

			this.AdvanceDefaultFrame();
			startedToReact.Should().BeTrue();

			shouldContinue = true;

			this.AdvanceDefaultFrame();

			mayCompleteReaction = true;
			this.AdvanceDefaultFrame();

			reactionCompleted.Should().BeTrue();
			task.IsCompleted.Should().BeTrue();
		}

		[Test]
		public async Task Until_DoesNotExecute_IfConditionIsMetFirst()
		{
			var cond1 = false;
			var untilCond = false;
			var cond2 = false;

			var firstCompleted = false;
			var secondCompleted = false;

			var task = WaitForCondition("Wait for first cond", () => cond1)
				.ThenRespondWithAction("First response", _ => firstCompleted = true)
				.Optionally()
				.Until(WaitForCondition("Until cond", () => untilCond))
				.ThenRespondWith("Second response", WaitForCondition("Second cond", () => cond2)
					.ExpectWithinSeconds(1)
					.ContinueWith(_ => DoAndReturn("Set second completed", () => secondCompleted = true)))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			untilCond = true;
			this.AdvanceDefaultFrame();

			cond1 = true;

			// A couple of extra frames to be safe
			this.AdvanceDefaultFrame();
			this.AdvanceDefaultFrame();
			this.AdvanceDefaultFrame();
			firstCompleted.Should().BeFalse();
			secondCompleted.Should().BeFalse();

			cond2 = true;
			this.AdvanceDefaultFrame();

			await AwaitTaskCompletionForUnity(task);

			(firstCompleted, secondCompleted, task.IsCompleted)
				.Should().Be((false, true, true));
		}

		[Test]
		public async Task Until_TimesOut_AsExpected()
		{
			var completed = false;

			var task = Never
				.ThenRespondWithAction("complete", _ => completed = true)
				.Optionally()
				.Until(Never)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.Scheduler.AdvanceFrame(OneSecond);

			completed.Should().BeFalse();
			(await AwaitFailureExceptionForUnity(task)).Should().NotBeNull();
		}

		[Test]
		public void Until_Description_MatchesExpected()
		{
			var state = Never
				.ThenRespondWithAction("complete", _ => { })
				.Optionally()
				.Until(Never)
				.CreateState();

			StateAssert.StringContainsInOrder(state.ToString())
				.Details("UNTIL")
				.NotStarted("Never")
				.Details("RESPOND TO ANY OF")
				.NotStarted("complete");
		}
	}
}
