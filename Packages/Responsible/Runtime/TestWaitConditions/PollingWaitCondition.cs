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
			Func<T> getConditionState,
			Func<T, bool> condition,
			[CanBeNull] Action<StateStringBuilder> extraContext,
			SourceContext sourceContext)
			: base(() => new State(description, getConditionState, condition, extraContext, sourceContext))
		{
		}

		private class State : TestOperationState<T>, IDiscreteWaitConditionState
		{
			private readonly Func<T> getConditionState;
			private readonly Func<T, bool> condition;

			public string Description { get; }
			public Action<StateStringBuilder> ExtraContext { get; }

			public State(
				string description,
				Func<T> getConditionState,
				Func<T, bool> condition,
				[CanBeNull] Action<StateStringBuilder> extraContext,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.Description = description;
				this.getConditionState = getConditionState;
				this.condition = condition;
				this.ExtraContext = extraContext;
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext) => runContext
				.PollObservable
				.StartWith(Unit.Default) // Allow immediate completion
				.Select(_ => this.getConditionState())
				.Where(this.condition)
				.Take(1);

			public override void BuildDescription(StateStringBuilder builder) => builder.AddWait(this);
		}
	}
}