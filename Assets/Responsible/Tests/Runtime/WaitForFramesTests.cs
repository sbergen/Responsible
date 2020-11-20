using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class WaitForFramesTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator WaitForFrames_CompletesAfterTimeout()
		{
			var completed = false;
			using (WaitForFrames(2).ToObservable(this.Executor).Subscribe(_ => completed = true))
			{
				Assert.IsFalse(completed);
				yield return null; // This frame
				Assert.IsFalse(completed);
				yield return null; // First frame
				Assert.IsFalse(completed);
				yield return null; // Second frame
				Assert.IsTrue(completed);
			}
		}

		[UnityTest]
		public IEnumerator WaitForFrames_CompletesAfterThisFrame_WithZeroFrames()
		{
			var completed = false;
			using (WaitForFrames(0).ToObservable(this.Executor).Subscribe(_ => completed = true))
			{
				Assert.IsFalse(completed);
				yield return null; // This frame
				Assert.IsTrue(completed);
			}
		}

		[UnityTest]
		public IEnumerator WaitForFrames_ContainsCorrectStatusInDescription()
		{
			var state = WaitForFrames(0).CreateState();
			StringAssert.Contains("[ ]", state.ToString());
			using (state.ToObservable(this.Executor).Subscribe())
			{
				StringAssert.Contains("[.]", state.ToString());
				yield return null;
				StringAssert.Contains("[✓]", state.ToString());
			}
		}
	}
}