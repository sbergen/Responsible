using System;
using System.Collections.Generic;
using System.Linq;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class AllOfWaitCondition<T> : TestWaitConditionBase<T[]>
	{
		public AllOfWaitCondition(IReadOnlyList<ITestWaitCondition<T>> conditions)
		: base (() => new State(conditions))
		{
		}

		private class State : OperationState<T[]>
		{
			private readonly IReadOnlyList<IOperationState<T>> conditions;

			public State(IReadOnlyList<ITestWaitCondition<T>> conditions)
			{
				this.conditions = conditions.Select(c => c.CreateState()).ToList();
			}

			protected override IObservable<T[]> ExecuteInner(RunContext runContext) => this.conditions
				.Select(cond => cond.Execute(runContext))
				.WhenAll();

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddParentWithChildren(
					"WAIT FOR ALL OF",
					this,
					this.conditions);
		}
	}
}