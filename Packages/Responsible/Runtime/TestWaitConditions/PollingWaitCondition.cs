using System;
using JetBrains.Annotations;
using Responsible.Context;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class PollingWaitCondition<T> : ITestWaitCondition<T>
	{
		private readonly Func<bool> condition;
		private readonly Func<T> makeResult;
		private readonly string description;
		[CanBeNull] private readonly Action<ContextStringBuilder> extraContext;

		public void BuildDescription(ContextStringBuilder builder) => builder.Add(this.description);

		// This is useful when waiting for multiple conditions
		public void BuildFailureContext(ContextStringBuilder builder) =>
			builder.AddWithNested(this.description, this.extraContext);

		public IObservable<T> WaitForResult(RunContext runContext, WaitContext waitContext) =>
			runContext.Executor.PollObservable
				.StartWith(Unit.Default) // Allow immediate completion
				.Select(_ => this.condition())
				.Where(fulfilled => fulfilled)
				.Take(1)
				.Do(_ => waitContext.MarkAsCompleted(this))
				.Select(_ => this.makeResult());

		public PollingWaitCondition(
			Func<bool> condition, string description, Func<T> makeResult, Action<ContextStringBuilder> extraContext = null)
		{
			this.description = description;
			this.condition = condition;
			this.makeResult = makeResult;
			this.extraContext = extraContext;
		}
	}
}