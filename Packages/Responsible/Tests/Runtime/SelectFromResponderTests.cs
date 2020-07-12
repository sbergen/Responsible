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

		[SetUp]
		public void SetUp()
		{
			this.responder = new ConditionResponder<int>(1, 2);
			this.responder.Responder
				.Select(i => i * 2)
				.ExpectWithinSeconds(1)
				.ToObservable()
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
		public IEnumerator SelectFromResponder_PublishesCorrectError_WhenExceptionThrown()
		{
			this.responder.AllowCompletionWithError(new Exception("Fail!"));
			yield return null;
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		[UnityTest]
		public IEnumerator SelectFromResponder_ContainsFailureDetails_WhenFailed()
		{
			this.responder.AllowCompletionWithError(new Exception("Fail!"));
			yield return null;

			// This is a bit curious, as the select didn't really fail, its source just failed...
			StringAssert.Contains(
				"[!] SELECT",
				this.Error.Message,
				"Should contain error for select");

			StringAssert.Contains(
				ConditionResponder.WaitForCompletionDescription,
				this.Error.Message,
				"Should contain responder details");
		}
	}
}