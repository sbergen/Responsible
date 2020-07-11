using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	/// <summary>
	/// Provides "type erasure" for wait conditions. We can't use Object as the generic type,
	/// as value types don't derive from it (no Any-type in C#)
	/// </summary>
	internal class UnitWaitCondition<T> : TestWaitConditionBase<Unit>
	{
		public UnitWaitCondition(ITestWaitCondition<T> condition)
			: base(() => new State(condition))
		{
		}

		private class State : OperationState<Unit>
		{
			private readonly IOperationState<T> condition;

			public State(ITestWaitCondition<T> condition)
			{
				this.condition = condition.CreateState();
			}

			protected override IObservable<Unit> ExecuteInner(RunContext runContext) =>
				this.condition.Execute(runContext).AsUnitObservable();

			public override void BuildFailureContext(StateStringBuilder builder) =>
				this.condition.BuildFailureContext(builder);
		}
	}
}