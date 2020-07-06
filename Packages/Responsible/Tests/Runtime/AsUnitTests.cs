using System;
using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class AsUnitTests : ResponsibleTestBase
	{
		private bool complete;
		private bool completed;

		private ITestWaitCondition<bool> waitForComplete;
		private ITestInstruction<bool> setCompleted;
		private ITestInstruction<int> throwError;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			this.waitForComplete = WaitForCondition("Wait", () => this.complete, () => this.complete);
			this.setCompleted = Do(() => this.completed = true);
			this.throwError = Do<int>(() => throw new Exception(""));
		}

		[SetUp]
		public void SetUp()
		{
			this.completed = this.complete = false;
		}

		[Test]
		public void UnitTestInstruction_DoesNotThrow_WhenSuccessful()
		{
			Assert.AreEqual(
				Unit.Default,
				this.setCompleted.AsUnitInstruction().Execute().Wait(TimeSpan.Zero));
		}

		[Test]
		public void UnitTestInstruction_DoesNotThrow_WhenError()
		{
			Assert.Throws<AssertionException>(() =>
				this.throwError.AsUnitInstruction().Execute().Wait(TimeSpan.Zero));
		}

		[UnityTest]
		public IEnumerator UnitWaitCondition_Completes_WhenCompleted()
		{
			this.waitForComplete
				.AsUnitCondition()
				.ExpectWithinSeconds(10)
				.Execute()
				.Subscribe(_ => this.completed = true);

			yield return null;
			Assert.IsFalse(this.completed);

			this.complete = true;
			yield return null;
			Assert.IsTrue(this.completed);
		}

		[UnityTest]
		public IEnumerator UnitWaitCondition_PublishesError_WhenTimedOut()
		{
			Never
				.AsUnitCondition()
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(Nop, this.StoreError);

			yield return null;
			Assert.IsNull(this.Error);

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}

		[UnityTest]
		public IEnumerator UnitTestResponder_Completes_WhenCompleted()
		{
			this.waitForComplete
				.ThenRespondWith("Respond", this.setCompleted)
				.AsUnitResponder()
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe();

			yield return null;
			Assert.IsFalse(this.completed);

			this.complete = true;
			yield return null;
			Assert.IsTrue(this.completed);
		}

		[UnityTest]
		public IEnumerator UnitTestResponder_PublishesError_OnError()
		{
			this.waitForComplete
				.ThenRespondWith("Respond", this.throwError)
				.AsUnitResponder()
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(Nop, this.StoreError);

			yield return null;
			Assert.IsNull(this.Error);

			this.complete = true;
			yield return null;
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}
	}
}