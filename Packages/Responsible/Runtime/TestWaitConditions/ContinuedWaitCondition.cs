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
			Func<TFirst, ITestWaitCondition<TSecond>> continuation)
			: base(() => new State(first, continuation))
		{
		}

		private class State : OperationState<TSecond>
		{
			private readonly IOperationState<TFirst> first;
			private readonly Func<TFirst, ITestWaitCondition<TSecond>> continuation;

			[CanBeNull] private IOperationState<TSecond> second;

			public State(ITestWaitCondition<TFirst> first, Func<TFirst, ITestWaitCondition<TSecond>> continuation)
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

			public override void BuildFailureContext(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, this.second);
		}
	}
}