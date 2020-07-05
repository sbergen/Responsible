using System;
using System.Collections.Generic;
using System.Linq;
using Responsible.Context;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class AllOfWaitCondition<T> : ITestWaitCondition<T>
	{
		private readonly ITestWaitCondition<T> primary;
		private readonly IReadOnlyList<ITestWaitCondition<Unit>> secondaries;

		private IEnumerable<ITestOperationContext> AllConditions => this.secondaries
			.Cast<ITestOperationContext>()
			.Prepend(this.primary);

		public IObservable<T> WaitForResult(RunContext runContext, WaitContext waitContext) => Observable
			.CombineLatest(
				this.primary.WaitForResult(runContext, waitContext),
				this.secondaries
					.Select(s => s.WaitForResult(runContext, waitContext))
					.WhenAll(),
				(result, _) => result);

		public void BuildFailureContext(ContextStringBuilder builder) => this.BuildDescription(builder);

		public void BuildDescription(ContextStringBuilder builder) =>
			builder.Add("ALL OF", this.AllConditions);

		public AllOfWaitCondition(
			ITestWaitCondition<T> primary,
			params ITestWaitCondition<Unit>[] secondaries)
		{
			this.primary = primary;
			this.secondaries = secondaries;
		}
	}
}