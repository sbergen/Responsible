using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;
using static Responsible.RF;

namespace Responsible.Tests.Runtime
{
	public class RunTests
	{
		private bool complete;

		[SetUp]
		public void SetUp()
		{
			this.complete = false;
		}

		[UnityTest]
		public IEnumerator RunCoroutine_Completes_WhenCoroutineIsComplete()
		{
			var completed = false;

			RunCoroutine(
					"Wait for completion",
					10,
					this.WaitForComplete)
				.Execute()
				.Subscribe(_ => completed = true);

			yield return null;
			Assert.IsFalse(completed);

			this.complete = true;
			yield return null;
			Assert.IsTrue(completed);
		}

		private IEnumerator WaitForComplete()
		{
			while (!this.complete)
			{
				yield return null;
			}
		}
	}
}