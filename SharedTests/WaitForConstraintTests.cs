using System;
using NUnit.Framework;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests
{
	public class WaitForConstraintTests : ResponsibleTestBase
	{
		[Test]
		public void WaitForConstraint_Completed_WhenConstraintTrue()
		{
			var mayComplete = false;
			var task = WaitForConstraint(
					"mayComplete",
					() => mayComplete, Is.True)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			Assert.IsFalse(task.IsCompleted, "Should not complete until constraint is true");

			mayComplete = true;
			this.AdvanceDefaultFrame();

			Assert.IsTrue(task.IsCompleted, "Should complete after constraint is true");
		}

		[Test]
		public void WaitForConstraint_IsReferentiallyTransparent()
		{
			// We had a bug where using .Not would cause the constraint to be mutated,
			// due to being resolved multiple times.
			// This test was added to ensure there are no regressions here.

			var mayComplete = false;
			var instruction = WaitForConstraint(
					"mayComplete",
					() => mayComplete, Is.Not.False)
				.ExpectWithinSeconds(1);

			// Reuse the condition with ContinueWith
			var task = instruction.ContinueWith(instruction).ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			mayComplete = true;
			this.AdvanceDefaultFrame();

			Assert.IsTrue(task.IsCompleted, "Should complete after constraint is true");
		}

		[Test]
		public void WaitForConstraint_HasExpectedDescription()
		{
			var str = "BAR";
			var state = WaitForConstraint(
				"Some string",
				() => str,
				Does.StartWith("FOO")).CreateState();
			Assert.AreEqual(
				@"[ ] Some string: String starting with ""FOO""",
				state.ToString());
		}

		[Test]
		public void WaitForConstraint_HasDetails_OnTimeout()
		{
			var str = "BAR";
			var task = WaitForConstraint(
				"Some string",
				() => str,
				Does.StartWith("FOO"))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.TimeProvider.AdvanceFrame(OneSecond);

			var error = GetAssertionException(task);
			Assert.That(error.Message, Does.Contain("Expected").And.Contain("But was"));
		}

		[Test]
		public void WaitForConstraint_HandlesErrorGracefully()
		{
			var task = WaitForConstraint<string>(
					"Some string",
					() => throw new Exception("Fail!"),
					Does.StartWith("FOO"))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			var error = GetAssertionException(task);
			Assert.That(
				error.Message,
				Does.Contain("[!] Some string").And.Contain("Fail!"));
		}
	}
}
