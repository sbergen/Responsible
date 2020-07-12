using System;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class PollingWaitCondition<T> : TestWaitConditionBase<T>
	{
		public PollingWaitCondition(
			string description,
			Func<bool> condition,
			Func<T> makeResult,
			Action<StateStringBuilder> extraContext = null)
			: base(() => new State(description, condition, makeResult, extraContext))
		{
		}

		private class State : OperationState<T>
		{
			private readonly string description;
			private readonly Func<bool> condition;
			private readonly Func<T> makeResult;
			[CanBeNull] private readonly Action<StateStringBuilder> extraContext;

			public State(
				string description,
				Func<bool> condition,
				Func<T> makeResult,
				[CanBeNull] Action<StateStringBuilder> extraContext)
			{
				this.description = description;
				this.condition = condition;
				this.makeResult = makeResult;
				this.extraContext = extraContext;
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext) => runContext
				.PollObservable
				.StartWith(Unit.Default) // Allow immediate completion
				.Select(_ => this.condition())
				.Where(fulfilled => fulfilled)
				.Take(1)
				.Select(_ => this.makeResult());

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddWait(this.description, this, this.extraContext);
		}
	}
}