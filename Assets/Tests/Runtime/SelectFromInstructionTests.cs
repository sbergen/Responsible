using System;
using NUnit.Framework;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class SelectFromInstructionTests : ResponsibleTestBase
	{
		[Test]
		public void SelectFromInstruction_GetsApplied_WhenSuccessful()
		{
			var result = Return(2)
				.Select(val => val * 2)
				.ToObservable(this.Executor)
				.Wait();
			Assert.AreEqual(4, result);
		}

		[Test]
		public void SelectFromInstruction_PublishesCorrectError_WhenExceptionThrown()
		{
			var observable = Return(2)
				.Select<int, int>(_ => throw new Exception("Fail!"))
				.ToObservable(this.Executor);
			Assert.Throws<AssertionException>(() => observable.Wait());
		}

		[Test]
		public void SelectFromInstruction_ContainsFailureDetails_WhenFailed()
		{
			Return(2)
				.Select<int, int>(_ => throw new Exception("Fail!"))
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			StringAssert.Contains(
				"[!] SELECT",
				this.Error.Message);
		}

		[Test]
		public void SelectFromInstruction_ContainsCorrectDetails_WhenInstructionFailed()
		{
			DoAndReturn<int>("Throw", () => throw new Exception("Fail!"))
				.Select(i => i)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			StringAssert.Contains(
				"[ ] SELECT",
				this.Error.Message,
				"Should not contain error for select");

			StringAssert.Contains(
				"[!] Throw",
				this.Error.Message,
				"Should contain error for instruction");
		}
	}
}