using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class DocumentationExample : ResponsibleTestBase
	{
		class Foo
		{
			public bool IsReady { get; set; }

			public void Consume(Bar bar)
            {
                var unused = bar;
				throw new Exception("Something failed");
			}

			public void ContinueOperation()
			{
			}
		}

		class Bar
		{
			public bool IsCompleted { get; set; }
		}

		[UnityTest]
		public IEnumerator CreateDocumentationFailure()
		{
			var foo = new Foo();
			var bar = new Bar();

			WaitForCondition("Foo to be ready", () => foo.IsReady)
				.AndThen(WaitForCondition("Bar to be completed", () => bar.IsCompleted))
				.ThenRespondWith("Consume bar", _ => foo.Consume(bar))
				.ExpectWithinSeconds(10)
				.ContinueWith(Do("Continue operation", foo.ContinueOperation))
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);

			yield return null;
			this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(Time.deltaTime));
			yield return null;
			this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(Time.deltaTime));

			foo.IsReady = true;

			yield return null;
			this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(Time.deltaTime));
			yield return null;
			this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(Time.deltaTime));
			yield return null;
			this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(Time.deltaTime));
			yield return null;
			this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(Time.deltaTime));

			bar.IsCompleted = true;

			yield return null;
			this.Scheduler.AdvanceBy(TimeSpan.FromSeconds(Time.deltaTime));

			Debug.Log(this.Error.Message.Replace(
				nameof(TestInstruction.ToObservable),
				nameof(TestInstruction.ToYieldInstruction)));
		}
	}
}