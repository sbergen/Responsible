using System;
using System.Collections;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using UniRx;
using UnityEngine.TestTools;

namespace Responsible.Tests.Runtime
{
	public class SelectFromResponderTests : ResponsibleTestBase
	{
		private ConditionResponder<int> responder;
		private int? result;
		private Func<int, int> selector;

		[SetUp]
		public void SetUp()
		{
			this.selector = i => i * 2;
			this.responder = new ConditionResponder<int>(1, 2);
			this.responder.Responder
				.Select(i => this.selector(i))
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(r => this.result = r, this.StoreError);
		}

		[UnityTest]
		public IEnumerator SelectFromResponder_GetsApplied_WhenSuccessful()
		{
			this.responder.AllowFullCompletion();
			yield return null;
			Assert.AreEqual(4, this.result);
		}

		[UnityTest]
		public IEnumerator SelectFromResponder_PublishesCorrectError_WhenResponderFails()
		{
			this.responder.AllowCompletionWithError(new Exception("Fail!"));
			yield return null;
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		[UnityTest]
		public IEnumerator SelectFromResponder_ContainsFailureDetails_WhenResponderFailed()
		{
			this.responder.AllowCompletionWithError(new Exception("Fail!"));
			yield return null;

			StringAssert.Contains(
				"[ ] SELECT",
				this.Error.Message,
				"Should Not have started select");

			StringAssert.Contains(
				ConditionResponder.WaitForCompletionDescription,
				this.Error.Message,
				"Should contain responder details");
		}

		[UnityTest]
		public IEnumerator SelectFromResponder_ContainsCorrectDetails_WhenSelectFails()
		{
			this.selector = _ => throw new Exception("Fail!");
			this.responder.AllowFullCompletion();
			yield return null;

			StringAssert.Contains(
				"[!] SELECT",
				this.Error.Message,
				"Should contain error for select");

			StringAssert.Contains(
				"Failed with:",
				this.Error.Message,
				"Should contain failure details for select");

			StringAssert.DoesNotContain(
				ConditionResponder.WaitForCompletionDescription,
				this.Error.Message,
				"Should not contain responder details");
		}
	}
}