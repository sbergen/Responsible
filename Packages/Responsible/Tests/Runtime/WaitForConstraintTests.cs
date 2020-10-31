using System;
using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class WaitForConstraintTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator WaitForConstraint_Completed_WhenConstraintTrue()
		{
			var mayComplete = false;
			var completed = false;
			WaitForConstraint(
					"mayComplete",
					() => mayComplete, Is.True)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(r => completed = r);

			yield return null;
			Assert.IsFalse(completed, "Should not complete until constraint is true");

			mayComplete = true;
			yield return null;

			Assert.IsTrue(completed, "Should complete after constraint is true");
		}

		[UnityTest]
		public IEnumerator WaitForConstraint_IsReferentiallyTransparent()
		{
			// We had a bug where using .Not would cause the constraint to be mutated,
			// due to being resolved multiple times.
			// This test was added to ensure there are no regressions here.

			var mayComplete = false;
			var completed = false;
			var instruction = WaitForConstraint(
					"mayComplete",
					() => mayComplete, Is.Not.False)
				.ExpectWithinSeconds(1);

			// Reuse the condition with ContinueWith
			instruction.ContinueWith(instruction)
				.ToObservable(this.Executor)
				.Subscribe(r => completed = r);

			yield return null;
			mayComplete = true;
			yield return null;

			Assert.IsTrue(completed, "Should complete after constraint is true");
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

		[UnityTest]
		public IEnumerator WaitForConstraint_HasDetails_OnTimeout()
		{
			var str = "BAR";
			WaitForConstraint(
				"Some string",
				() => str,
				Does.StartWith("FOO"))
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.That(this.Error.Message, Does.Contain("Expected").And.Contain("But was"));
		}

		[UnityTest]
		public IEnumerator WaitForConstraint_HandlesErrorGracefully()
		{
			WaitForConstraint<string>(
					"Some string",
					() => throw new Exception("Fail!"),
					Does.StartWith("FOO"))
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			yield return null;

			Assert.That(
				this.Error.Message,
				Does.Contain("[!] Some string").And.Contain("Fail!"));
		}
	}
}