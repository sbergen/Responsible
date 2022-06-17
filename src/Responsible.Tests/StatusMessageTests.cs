using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class StatusMessageTests : ResponsibleTestBase
	{
		[Test]
		public void WaitMessage_ContainsCorrectDetails()
		{
			var state = Never.ExpectWithinSeconds(1).CreateState();

			this.AdvanceDefaultFrame(); // Should not count in time

			state.ToTask(this.Executor); // Start execution

			StateAssert.StringContainsInOrder(state.ToString())
				.Waiting("Never")
				.Details(@"Started 0\.00 s ≈ 0 frames ago");

			this.AdvanceDefaultFrame();
			StateAssert.StringContainsInOrder(state.ToString())
				.Waiting("Never")
				.Details($@"Started {OneFrame.TotalSeconds:0.00} s ≈ 1 frames ago");
		}

		[Test]
		public void CompletedMessage_ContainsCorrectDetails()
		{
			var complete = false;
			// ReSharper disable once AccessToModifiedClosure
			var state = WaitForCondition("Wait...", () => complete)
				.ExpectWithinSeconds(1)
				.CreateState();

			this.AdvanceDefaultFrame(); // Should not count in time

			// Start execution, completes on next frame
			state.ToTask(this.Executor);
			complete = true;
			this.AdvanceDefaultFrame();

			StateAssert.StringContainsInOrder(state.ToString())
				.Completed("Wait...")
				.Details($@"Completed in {OneFrame.TotalSeconds:0.00} s ≈ 1 frames");
		}

		[Test]
		public async Task CanceledMessage_ContainsCorrectDetails()
		{
			using (var cts = new CancellationTokenSource())
			{
				var state = Never.ExpectWithinSeconds(1).CreateState();

				this.AdvanceDefaultFrame(); // Should not count in time

				// Start execution, canceled after one frame
				var task = state.ToTask(this.Executor, cts.Token);
				this.AdvanceDefaultFrame();
				cts.Cancel();

				await AwaitFailureExceptionForUnity(task);

				StateAssert.StringContainsInOrder(state.ToString())
					.Canceled("Never")
					.Details($@"Canceled after {OneFrame.TotalSeconds:0.00} s ≈ 1 frames");
			}
		}
	}
}
