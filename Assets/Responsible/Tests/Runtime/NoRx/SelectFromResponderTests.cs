using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.NoRx;
using Responsible.Tests.Runtime.NoRx.Utilities;

namespace Responsible.Tests.Runtime.NoRx
{
	public class SelectFromResponderTests : ResponsibleTestBase
	{
		private ConditionResponder<int> responder;
		private Task<int> task;
		private Func<int, int> selector;

		[SetUp]
		public void SetUp()
		{
			this.selector = i => i * 2;
			this.responder = new ConditionResponder<int>(1, 2);
			this.task = this.responder.Responder
				.Select(i => this.selector(i))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);
		}

		[Test]
		public void SelectFromResponder_GetsApplied_WhenSuccessful()
		{
			this.responder.AllowFullCompletion();
			this.AdvanceDefaultFrame();
			Assert.AreEqual(4, this.task.Result);
		}

		[Test]
		public void SelectFromResponder_PublishesCorrectError_WhenResponderFails()
		{
			this.responder.AllowCompletionWithError(new Exception("Fail!"));
			this.AdvanceDefaultFrame();
			Assert.IsNotNull(GetAssertionException(this.task));
		}

		[Test]
		public void SelectFromResponder_ContainsFailureDetails_WhenResponderFailed()
		{
			this.responder.AllowCompletionWithError(new Exception("Fail!"));
			this.AdvanceDefaultFrame();

			var error = GetAssertionException(this.task);

			StringAssert.Contains(
				"[ ] SELECT",
				error.Message,
				"Should Not have started select");

			StringAssert.Contains(
				ConditionResponder.WaitForCompletionDescription,
				error.Message,
				"Should contain responder details");
		}

		[Test]
		public void SelectFromResponder_ContainsCorrectDetails_WhenSelectFails()
		{
			this.selector = _ => throw new Exception("Fail!");
			this.responder.AllowFullCompletion();
			this.AdvanceDefaultFrame();

			var error = GetAssertionException(this.task);

			StringAssert.Contains(
				"[!] SELECT",
				error.Message,
				"Should contain error for select");

			StringAssert.Contains(
				"Failed with:",
				error.Message,
				"Should contain failure details for select");

			StringAssert.DoesNotContain(
				ConditionResponder.WaitForCompletionDescription,
				error.Message,
				"Should not contain responder details");
		}
	}
}
