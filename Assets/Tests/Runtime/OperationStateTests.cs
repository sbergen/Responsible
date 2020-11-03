using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class OperationStateTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator ReusingSameInstruction_ProvidesSeparateState()
		{
			bool condition = true;

			bool WaitForIt()
			{
				if (condition)
				{
					condition = false;
					return true;
				}

				return condition;
			}

			var wait = WaitForCondition("Wait for it", WaitForIt);

			wait.AndThen(wait)
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.That(
				this.Error.Message,
				Does.Contain("[.] Wait for it").And.Contain("[✓] Wait for it"));
		}

	}
}