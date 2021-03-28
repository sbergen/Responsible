using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;

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

			protected override async Task<TSecond> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				var firstResult = await this.first.Execute(runContext, cancellationToken);
				this.second = this.continuation(firstResult).CreateState();
				return await this.second.Execute(runContext, cancellationToken);
			}

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, this.second);
		}
	}
}
