using System;
using NUnit.Framework;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class SelectFromWaitConditionTests : ResponsibleTestBase
	{
		[Test]
		public void SelectFromCondition_GetsApplied_WhenSuccessful()
		{
			var result = ImmediateTrue
				.Select(r => r ? 1 : 0)
				.ExpectWithinSeconds(1)
				.ToObservable()
				.Wait();

			Assert.AreEqual(1, result);
		}

		[Test]
		public void SelectFromCondition_PublishesError_WhenExceptionThrown()
		{
			Assert.Throws<AssertionException>(() =>
				ImmediateTrue
					.Select<bool, int>(r => throw new Exception("Fail!"))
					.ExpectWithinSeconds(1)
					.ToObservable()
					.Wait());
		}

		[Test]
		public void SelectFromCondition_ContainsFailureDetails_WhenFailed()
		{
			ImmediateTrue
				.Select<bool, int>(r => throw new Exception("Fail!"))
				.ExpectWithinSeconds(1)
				.ToObservable()
				.Subscribe(Nop, this.StoreError);

			StringAssert.Contains(
				"[!] SELECT",
				this.Error.Message);
		}

		[Test]
		public void SelectFromCondition_ContainsCorrectDetails_WhenConditionFailed()
		{
			WaitForCondition("Throw", () => throw new Exception("Fail!"))
				.Select(r => r)
				.ExpectWithinSeconds(1)
				.ToObservable()
				.Subscribe(Nop, this.StoreError);

			StringAssert.Contains(
				"[ ] SELECT",
				this.Error.Message,
				"Should not contain error for Select");

			StringAssert.Contains(
				"[!] Throw",
				this.Error.Message,
				"Should contain error for condition");
		}
	}
}