using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class SelectFromInstructionTests : ResponsibleTestBase
	{
		[Test]
		public void SelectFromInstruction_GetsApplied_WhenSuccessful()
		{
			var result = Return(2)
				.Select(val => val * 2)
				.ToTask(this.Executor)
				.AssertSynchronousResult();
			Assert.AreEqual(4, result);
		}

		[Test]
		public async Task SelectFromInstruction_PublishesCorrectError_WhenExceptionThrown()
		{
			var task = Return(2)
				.Select<int, int>(_ => throw new Exception("Fail!"))
				.ToTask(this.Executor);
			Assert.IsNotNull(await AwaitFailureExceptionForUnity(task));
		}

		[Test]
		public async Task SelectFromInstruction_ContainsFailureDetails_WhenFailed()
		{
			var task = Return(2)
				.Select<int, int>(_ => throw new Exception("Fail!"))
				.ToTask(this.Executor);

			var exception = await AwaitFailureExceptionForUnity(task);
			StateAssert.StringContainsInOrder(exception.Message)
				.Failed("SELECT")
				.FailureDetails();
		}

		[Test]
		public async Task SelectFromInstruction_ContainsCorrectDetails_WhenInstructionFailed()
		{
			var task = DoAndReturn<int>("Throw", () => throw new Exception("Fail!"))
				.Select(i => i)
				.ToTask(this.Executor);

			var exception = await AwaitFailureExceptionForUnity(task);
			StateAssert.StringContainsInOrder(exception.Message)
				.Failed("Throw")
				.FailureDetails()
				.NotStarted("SELECT");
		}
	}
}
