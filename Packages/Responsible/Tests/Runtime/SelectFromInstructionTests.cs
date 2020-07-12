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
				.ToObservable()
				.Wait();
			Assert.AreEqual(4, result);
		}

		[Test]
		public void SelectFromInstruction_PublishesCorrectError_WhenExceptionThrown()
		{
			var observable = Return(2)
				.Select<int, int>(_ => throw new Exception("Fail!"))
				.ToObservable();
			Assert.Throws<AssertionException>(() => observable.Wait());
		}

		[Test]
		public void SelectFromInstruction_ContainsFailureDetails_WhenFailed()
		{
			Return(2)
				.Select<int, int>(_ => throw new Exception("Fail!"))
				.ToObservable()
				.Subscribe(Nop, this.StoreError);

			StringAssert.Contains(
				"[!] SELECT",
				this.Error.Message);
		}
	}
}