using System;
using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class CoroutineTests : ResponsibleTestBase
	{
		private static IEnumerator CompleteAfterTwoFrames()
		{
			yield return null;
			yield return null;
		}

		private static IEnumerator ThrowAfterOneFrame()
		{
			yield return null;
			throw new Exception("Fail!");
		}

		[UnityTest]
		public IEnumerator WaitForCoroutine_Completes_WhenExpected()
		{
			var completed = false;
			WaitForCoroutineMethod(CompleteAfterTwoFrames)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(_ => completed = true);

			Assert.IsFalse(completed, "Should not complete before any yields");
			yield return null;
			Assert.IsFalse(completed, "Should not complete after first yield");
			yield return null;
			Assert.IsTrue(completed, "Should complete after second yield");
		}

		[UnityTest]
		public IEnumerator WaitForCoroutine_PublishesErrorWithContext_WhenExpected()
		{
			var completed = false;
			WaitForCoroutineMethod(ThrowAfterOneFrame)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			Assert.IsFalse(completed, "Should not have error before yields");
			yield return null;

			Assert.IsInstanceOf<AssertionException>(this.Error, "Should have error after first yield");
			Assert.That(
				this.Error.Message,
				Does.Contain("[!] ThrowAfterOneFrame").And.Contain("Fail!"));
		}

		[Test]
		public void WaitForCoroutine_HasCorrectDescription_WhenConstructedWithDescription()
		{
			IEnumerator LocalCoroutine()
			{
				yield break;
			}

			var description = WaitForCoroutine("LocalCoroutine", LocalCoroutine).CreateState().ToString();
			Assert.That(description, Does.Contain("[ ] LocalCoroutine"));
		}
	}
}