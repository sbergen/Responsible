using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.RF;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class AndThenTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator AndThen_CompletesWhenAllComplete_WithReadySecondCondition()
		{
			var cond1 = false;
			var cond2 = false;
			var completed = false;

			WaitForCondition("First", () => cond1)
				.AndThen(WaitForCondition("Second", () =>
				{
					Assert.IsTrue(cond1);
					return cond2;
				}))
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(_ => completed = true);

			yield return null;
			Assert.IsFalse(completed);

			cond1 = true;
			yield return null;
			Assert.IsFalse(completed);

			cond2 = true;
			yield return null;
			yield return null; // TODO check why this is needed
			Assert.IsTrue(completed);
		}

		[UnityTest]
		public IEnumerator AndThen_CompletesWhenAllComplete_WithDeferredSecondCondition()
		{
			var cond1 = false;
			var cond2 = false;
			var completed = false;

			WaitForCondition("First", () => cond1)
				.AndThen(_ => WaitForCondition("Second", () =>
				{
					Assert.IsTrue(cond1);
					return cond2;
				}))
				.ExpectWithinSeconds(1)
				.Execute()
				.Subscribe(_ => completed = true);

			yield return null;
			Assert.IsFalse(completed);

			cond1 = true;
			yield return null;
			Assert.IsFalse(completed);

			cond2 = true;
			yield return null;
			yield return null; // TODO check why this is needed
			Assert.IsTrue(completed);
		}
	}
}