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

		public void BuildFailureContext(ContextStringBuilder builder) => builder
			.AddWithNested(
				builder.DescriptionForWait(this, this.description),
				this.extraContext);

		public IObservable<T> WaitForResult(RunContext runContext, WaitContext waitContext) =>
			waitContext.PollObservable
				.StartWith(Unit.Default) // Allow immediate completion
				.Select(_ => this.condition())
				.Where(fulfilled => fulfilled)
				.Take(1)
				.Select(_ => this.makeResult())
				.Do(_ => waitContext.MarkAsCompleted(this))
				.DoOnSubscribe(() => waitContext.MarkAsStarted(this));

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