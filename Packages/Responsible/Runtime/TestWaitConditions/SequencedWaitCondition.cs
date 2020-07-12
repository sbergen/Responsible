using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class SequencedWaitCondition<TFirst, TSecond> : TestWaitConditionBase<TSecond>
	{
		public SequencedWaitCondition(ITestWaitCondition<TFirst> first, ITestWaitCondition<TSecond> second)
			: base(() => new State(first, second))
		{
		}

		private class State : OperationState<TSecond>
		{
			private readonly IOperationState<TFirst> first;
			private readonly IOperationState<TSecond> second;

			public State(ITestWaitCondition<TFirst> first, ITestWaitCondition<TSecond> second)
			{
				this.first = first.CreateState();
				this.second = second.CreateState();
			}

			protected override IObservable<TSecond> ExecuteInner(RunContext runContext) =>
				this.first
					.Execute(runContext)
					.ContinueWith(_ => this.second.Execute(runContext));

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, this.second);
		}
	}
}