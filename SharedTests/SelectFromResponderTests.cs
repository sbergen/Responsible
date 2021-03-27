using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests.Utilities;

namespace Responsible.Tests
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
			Assert.AreEqual(4, this.task.AssertSynchronousResult());
		}

		[Test]
		public void SelectFromResponder_PublishesCorrectError_WhenResponderFails()
		{
			this.responder.AllowCompletionWithError(new Exception("Fail!"));
			this.AdvanceDefaultFrame();
			Assert.IsNotNull(GetFailureException(this.task));
		}

		[Test]
		public void SelectFromResponder_ContainsFailureDetails_WhenResponderFailed()
		{
			this.responder.AllowCompletionWithError(new Exception("Fail!"));
			this.AdvanceDefaultFrame();

			var error = GetFailureException(this.task);

			StateAssert.StringContainsInOrder(error.Message)
				.Details(ConditionResponder.WaitForCompletionDescription)
				.NotStarted("SELECT");
		}

		[Test]
		public void SelectFromResponder_ContainsCorrectDetails_WhenSelectFails()
		{
			this.selector = _ => throw new Exception("Fail!");
			this.responder.AllowFullCompletion();
			this.AdvanceDefaultFrame();

			var error = GetFailureException(this.task);

			StateAssert.StringContainsInOrder(error.Message)
				.Failed("SELECT")
				.FailureDetails()
				.Nowhere(ConditionResponder.WaitForCompletionDescription);
		}
	}
}
