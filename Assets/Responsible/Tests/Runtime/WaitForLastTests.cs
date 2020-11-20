using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class WaitForLastTests : ResponsibleTestBase
	{
		[Test]
		public void WaitForLast_Completes_WhenCompleted()
		{
			Subject<int> subject = new Subject<int>();
			int? result = null;

			using (WaitForLast("Wait for last", subject)
				.ExpectWithinSeconds(10)
				.ToObservable(this.Executor)
				.Subscribe(r => result = r))
			{
				Assert.IsNull(result);
				subject.OnNext(1);
				Assert.IsNull(result);
				subject.OnNext(2);
				Assert.IsNull(result);
				subject.OnCompleted();
				Assert.AreEqual(2, result);
			}
		}

		[Test]
		public void WaitForLast_PublishesError_OnEmpty()
		{
			using (WaitForLast("Wait for last on empty", Observable.Empty<int>())
				.ExpectWithinSeconds(10)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError))
			{
				Assert.IsInstanceOf<AssertionException>(this.Error);
			}
		}

		[UnityTest]
		public IEnumerator WaitForLast_TimesOut_WhenTimeoutExceeded()
		{
			using (WaitForLast("Wait for last on never", Observable.Never<int>())
				.ExpectWithinSeconds(1)
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError))
			{
				yield return null;
				Assert.IsNull(this.Error);

				this.Scheduler.AdvanceBy(OneSecond);
				yield return null;
				Assert.IsInstanceOf<AssertionException>(this.Error);
			}
		}
	}
}
