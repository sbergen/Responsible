using System.Collections;
using NUnit.Framework;
using Responsible.UnityTests.Utilities;
using UnityEngine;
using UnityEngine.TestTools;

namespace Responsible.UnityTests
{
	public class ToYieldInstructionTests : UnityTestBase
	{
		private CoroutineRunner coroutineRunner;
		private int? completedOnFrame;
		private bool mayComplete;

		[SetUp]
		public void SetUp()
		{
			this.completedOnFrame = null;
			this.mayComplete = false;
			this.coroutineRunner = CoroutineRunner.Create();
		}

		[TearDown]
		public void TearDown()
		{
			this.coroutineRunner.Destroy();
		}

		[UnityTest]
		public IEnumerator ToYieldInstruction_CompletesAsExpected()
		{
			this.coroutineRunner.StartCoroutine(this.YieldOneFrame());

			yield return Responsibly
				.WaitForCondition("may complete", () => this.mayComplete)
				.ExpectWithinSeconds(1)
				.ToYieldInstruction(this.Executor);

			// Completes one frame after
			Assert.AreEqual(this.completedOnFrame + 1, Time.frameCount);
		}

		private IEnumerator YieldOneFrame()
		{
			yield return null;
			this.mayComplete = true;
			this.completedOnFrame = Time.frameCount;
		}
	}
}
