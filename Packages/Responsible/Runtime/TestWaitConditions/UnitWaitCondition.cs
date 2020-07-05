using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestWaitConditions
{
	/// <summary>
	/// Provides "type erasure" for wait conditions. We can't use Object as the generic type,
	/// as value types don't derive from it (no Any-type in C#)
	/// </summary>
	internal class UnitWaitCondition<T> : ITestWaitCondition<Unit>
	{
		private readonly ITestWaitCondition<T> condition;

		public IObservable<Unit> WaitForResult(RunContext runContext, WaitContext waitContext) => this.condition
			.WaitForResult(runContext, waitContext)
			.AsUnitObservable();

		public void BuildDescription(ContextStringBuilder builder) => this.condition.BuildDescription(builder);
		public void BuildFailureContext(ContextStringBuilder builder) => this.condition.BuildFailureContext(builder);

		public UnitWaitCondition(ITestWaitCondition<T> condition)
		{
			this.condition = condition;
		}
	}
}