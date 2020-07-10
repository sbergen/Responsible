using System;
using System.Collections.Generic;
using System.Linq;
using Responsible.Context;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class AllOfWaitCondition<T> : ITestWaitCondition<T[]>
	{
		private readonly IReadOnlyList<ITestWaitCondition<T>> conditions;

		public IObservable<T[]> WaitForResult(RunContext runContext, WaitContext waitContext) => this.conditions
			.Select(cond => cond.WaitForResult(runContext, waitContext))
			.WhenAll();
		public void BuildFailureContext(ContextStringBuilder builder) => this.BuildDescription(builder);

		public void BuildDescription(ContextStringBuilder builder) =>
			builder.Add("ALL OF", this.conditions);

		public AllOfWaitCondition(
			params ITestWaitCondition<T>[] conditions)
		{
			this.conditions = conditions;
		}
	}
}