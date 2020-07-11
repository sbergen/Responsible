using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestWaitConditions
{
	public class ContinuedWaitCondition<TFirst, TSecond> : ITestWaitCondition<TSecond>
	{
		private readonly ITestWaitCondition<TFirst> first;
		private readonly Func<TFirst, ITestWaitCondition<TSecond>> continuation;

		public ContinuedWaitCondition(
			ITestWaitCondition<TFirst> first, Func<TFirst, ITestWaitCondition<TSecond>> continuation)
		{
			this.first = first;
			this.continuation = continuation;
		}

		public IObservable<TSecond> WaitForResult(RunContext runContext, WaitContext waitContext) => this.first
			.WaitForResult(runContext, waitContext)
			.ContinueWith(result =>
			{
				var next = this.continuation(result);
				waitContext.AddRelation(this, next);
				return next.WaitForResult(runContext, waitContext);
			});

		public void BuildDescription(ContextStringBuilder builder)
		{
			builder.Add("FIRST", this.first);
			builder.AddOptional(
				"AND THEN",
				builder.WaitContext.RelatedContexts(this));
		}

		public void BuildFailureContext(ContextStringBuilder builder) => this.BuildDescription(builder);
	}
}