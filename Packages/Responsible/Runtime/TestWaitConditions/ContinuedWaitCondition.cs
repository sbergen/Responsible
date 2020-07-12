using System;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class ContinuedWaitCondition<TFirst, TSecond> : TestWaitConditionBase<TSecond>
	{
		public ContinuedWaitCondition(
			ITestWaitCondition<TFirst> first,
			Func<TFirst, ITestWaitCondition<TSecond>> continuation,
			SourceContext sourceContext)
			: base(() => new State(first, continuation, sourceContext))
		{
		}

		private class State : TestOperationState<TSecond>
		{
			private readonly ITestOperationState<TFirst> first;
			private readonly Func<TFirst, ITestWaitCondition<TSecond>> continuation;

			[CanBeNull] private ITestOperationState<TSecond> second;

			public State(
				ITestWaitCondition<TFirst> first,
				Func<TFirst, ITestWaitCondition<TSecond>> continuation,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.first = first.CreateState();
				this.continuation = continuation;
			}

			protected override IObservable<TSecond> ExecuteInner(RunContext runContext) => this.first
				.Execute(runContext)
				.ContinueWith(result =>
				{
					this.second = this.continuation(result).CreateState();
					return this.second.Execute(runContext);
				});

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, this.second);
		}
	}
}