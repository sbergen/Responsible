using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;
using UnityEngine;
using static Responsible.RF;

namespace Responsible.Tests.Runtime
{
	public class ToYieldInstructionTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator ToYieldInstruction_CompletesAsExpected()
		{
			var cond = false;

			Observable.TimerFrame(1).Subscribe(_ => cond = true);
			var framesBefore = Time.frameCount;

			yield return WaitForCondition("cond", () => cond)
				.ExpectWithinSeconds(1)
				.ToYieldInstruction();

			// There's a one frame delay with completion
			Assert.AreEqual(framesBefore + 2, Time.frameCount);

		}

	}
}