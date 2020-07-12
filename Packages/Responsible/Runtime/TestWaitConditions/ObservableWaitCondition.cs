using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class ObservableWaitCondition<T> : TestWaitConditionBase<T>
	{
		public ObservableWaitCondition(
			string description,
			IObservable<T> observable,
			SourceContext sourceContext)
			: base(() => new State(description, observable, sourceContext))
		{
		}

		private class State : OperationState<T>
		{
			private readonly string description;
			private readonly IObservable<T> observable;

			public State(string description, IObservable<T> observable, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.description = description;
				this.observable = observable;
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext)
				=> this.observable.Last();

			public override void BuildDescription(StateStringBuilder builder)
				=> builder.AddWait(this.description, this);
		}
	}
}