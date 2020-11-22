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

		private class State : TestOperationState<T>, IDiscreteWaitConditionState
		{
			private readonly IObservable<T> observable;

			public string Description { get; }
			public Action<StateStringBuilder> ExtraContext => null;

			public State(string description, IObservable<T> observable, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.Description = description;
				this.observable = observable;
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext)
				=> this.observable.Last();

			public override void BuildDescription(StateStringBuilder builder) => builder.AddWait(this);
		}
	}
}