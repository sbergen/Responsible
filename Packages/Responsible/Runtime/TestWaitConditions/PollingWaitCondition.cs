using System;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class PollingWaitCondition<TCondition, TResult> : TestWaitConditionBase<TResult>
	{
		public PollingWaitCondition(
			string description,
			Func<TCondition> getConditionState,
			Func<TCondition, bool> condition,
			Func<TCondition, TResult> makeResult,
			[CanBeNull] Action<StateStringBuilder> extraContext,
			SourceContext sourceContext)
			: base(() => new State(description, getConditionState, condition, makeResult, extraContext, sourceContext))
		{
		}

		private class State : TestOperationState<TResult>, IDiscreteWaitConditionState
		{
			private readonly Func<TCondition> getConditionState;
			private readonly Func<TCondition, bool> condition;
			private readonly Func<TCondition, TResult> makeResult;

			public string Description { get; }
			[CanBeNull] public Action<StateStringBuilder> ExtraContext { get; }

			public State(
				string description,
				Func<TCondition> getConditionState,
				Func<TCondition, bool> condition,
				Func<TCondition, TResult> makeResult,
				[CanBeNull] Action<StateStringBuilder> extraContext,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.Description = description;
				this.getConditionState = getConditionState;
				this.condition = condition;
				this.makeResult = makeResult;
				this.ExtraContext = extraContext;
			}

			protected override IObservable<TResult> ExecuteInner(RunContext runContext) => runContext
				.PollObservable
				.StartWith(Unit.Default) // Allow immediate completion
				.Select(_ => this.getConditionState())
				.Where(this.condition)
				.Take(1)
				.Select(this.makeResult);

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddWait(this.Description, this, this.ExtraContext);
		}
	}
}