using System.Collections;
using NUnit.Framework;
using Responsible.State;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class StateLoggingTests : ResponsibleTestBase
	{
		private Subject<int> observable;
		private ITestOperationState<int> state;

		[SetUp]
		public void SetUp()
		{
			this.observable = new Subject<int>();
			this.state = WaitForLast(
					"Wait for value",
					this.observable)
				.ExpectWithinSeconds(1)
				.CreateState();
		}

		[Test]
		public void InitialState_ProducesCorrectOutput()
		{
			var stateString = this.state.ToString();
			StringAssert.Contains("[ ] Wait for value EXPECTED WITHIN", stateString);
		}

		[Test]
		public void ToObservable_ProducesCorrectOutput_AfterCompletion()
		{
			this.state.ToObservable(this.Executor).Subscribe();
			this.observable.OnNext(42);
			this.observable.OnCompleted();

			var stateString = this.state.ToString();
			StringAssert.Contains("[✓] Wait for value EXPECTED WITHIN", stateString);
		}

		[UnityTest]
		public IEnumerator ToYieldInstruction_ProducesCorrectOutput_AfterCompletion()
		{
			var instruction = this.state.ToYieldInstruction(this.Executor);
			this.observable.OnNext(42);
			this.observable.OnCompleted();

			yield return instruction;

			var stateString = this.state.ToString();
			StringAssert.Contains("[✓] Wait for value EXPECTED WITHIN", stateString);
		}
	}
}