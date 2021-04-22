using System;
using System.Threading;
using System.Threading.Tasks;
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
			private readonly Continuation<TFirst, TSecond> continuation;

			public State(
				ITestWaitCondition<TFirst> first,
				Func<TFirst, ITestWaitCondition<TSecond>> continuation,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.first = first.CreateState();
				this.continuation = new Continuation<TFirst, TSecond>(
					firstResult => continuation(firstResult).CreateState());
			}

			protected override async Task<TSecond> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				var firstResult = await this.first.Execute(runContext, cancellationToken);
				return await this.continuation.Execute(firstResult, runContext, cancellationToken);
			}

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, this.continuation.State);
		}
	}
}
