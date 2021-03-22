/* TODO
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;
using UnityEngine;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ToYieldInstructionTests : ResponsibleTestBase
	{

		[UnityTest]
		public IEnumerator ToYieldInstruction_CompletesAsExpected()
		{
			var cond = false;

			int completedOnFrame = default;
			Observable.TimerFrame(1).Subscribe(_ =>
			{
				completedOnFrame = Time.frameCount;
				cond = true;
			});

			yield return WaitForCondition("cond", () => cond)
				.ExpectWithinSeconds(1)
				.ToYieldInstruction(this.Executor);

			// Completes one frame after
			Assert.AreEqual(completedOnFrame + 1, Time.frameCount);
		}
	}
}
*/
