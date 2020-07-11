using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class ObservableWaitCondition<T> : ITestWaitCondition<T>
	{
		private readonly string description;
		private readonly IObservable<T> observable;

		public IObservable<T> WaitForResult(RunContext runContext, WaitContext waitContext) =>
			this.observable
				.Last()
				.DoOnSubscribe(() => waitContext.MarkAsStarted(this))
				.Do(_ => waitContext.MarkAsCompleted(this));

		public void BuildDescription(ContextStringBuilder builder) => builder.Add(this.description);

		public void BuildFailureContext(ContextStringBuilder builder)
			=> builder.AddWaitStatus(this, this.description);

		public ObservableWaitCondition(
			string description,
			IObservable<T> observable)
		{
			this.description = description;
			this.observable = observable;
		}
	}
}