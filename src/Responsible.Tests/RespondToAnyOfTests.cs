using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class RespondToAnyOfTests : ResponsibleTestBase
	{
		private ConditionResponder responder1;
		private ConditionResponder responder2;

		private bool mayComplete;
		private Task<object> task;

		[SetUp]
		public void SetUp()
		{
			this.mayComplete = false;
			this.responder1 = new ConditionResponder(1);
			this.responder2 = new ConditionResponder(1);

			this.task = RespondToAnyOf(
					this.responder1.Responder,
					this.responder2.Responder)
				.Until(WaitForCondition("Complete", () => this.mayComplete))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);
		}

		[Test]
		public async Task RespondToAnyOf_Completes_WhenEitherCompleted([Values] bool completeFirst)
		{
			this.responder1.MayComplete = true;
			this.responder2.MayComplete = true;

			if (completeFirst)
			{
				this.responder1.MayRespond = true;
			}
			else
			{
				this.responder2.MayRespond = true;
			}

			this.AdvanceDefaultFrame();

			this.mayComplete = true;

			this.AdvanceDefaultFrame();

			await AwaitTaskCompletionForUnity(this.task);

			(this.task.IsCompleted, this.responder1.CompletedRespond, this.responder2.CompletedRespond)
				.Should().Be((true, completeFirst, !completeFirst));
		}

		[Test]
		public async Task RespondToAnyOf_ConditionsDoNotExecute_WhenUntilConditionCompletesFirst()
		{
			this.responder1.MayComplete = true;
			this.responder2.MayComplete = true;

			this.AdvanceDefaultFrame();

			this.mayComplete = true;

			this.AdvanceDefaultFrame();

			this.responder1.MayRespond = true;
			this.responder2.MayRespond = true;

			this.AdvanceDefaultFrame();

			await AwaitTaskCompletionForUnity(this.task);

			(this.task.IsCompleted, this.responder1.StartedToRespond, this.responder2.StartedToRespond)
				.Should().Be((true, false, false));
		}

		[Test]
		public void RespondToAnyOf_ExecutesRespondersSequentially_WhenMultipleReadyToRespond()
		{
			this.responder1.MayRespond = true;
			this.responder2.MayRespond = true;

			this.AdvanceDefaultFrame();
			(this.responder1.StartedToRespond && this.responder2.StartedToRespond)
				.Should().BeFalse("only one responder should start while the other is executing");

			// Complete everything
			this.mayComplete = true;
			this.responder1.MayComplete = true;
			this.responder2.MayComplete = true;
			this.AdvanceDefaultFrame();

			(this.task.IsCompleted, this.responder1.CompletedRespond, this.responder2.CompletedRespond)
				.Should().Be((true, true, true));
		}

		// This exists to test the boxing conversion bypass case, when it's not necessary
		[Test]
		public void RespondToAnyOf_WorksCorrectlyWithReferenceTypes()
		{
			var expectedObject = new object();
			var instruction = RespondToAnyOf(
					ImmediateTrue.ThenRespondWith("Return object", Return(expectedObject)),
					this.responder1.Responder.BoxResult())
				.Until(Never)
				.ExpectWithinSeconds(1);

			Assert.DoesNotThrow(() => instruction.ToTask(this.Executor));
		}
	}
}
