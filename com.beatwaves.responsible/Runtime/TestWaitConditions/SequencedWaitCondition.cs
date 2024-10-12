using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestWaitConditions
{
	internal class SequencedWaitCondition<TFirst, TSecond> : TestWaitConditionBase<TSecond>
	{
		public SequencedWaitCondition(
			ITestWaitCondition<TFirst> first,
			ITestWaitCondition<TSecond> second,
			SourceContext sourceContext)
			: base(() => new State(first, second, sourceContext))
		{
		}

		private class State : TestOperationState<TSecond>
		{
			private readonly ITestOperationState<TFirst> first;
			private readonly ITestOperationState<TSecond> second;

			public State(
				ITestWaitCondition<TFirst> first,
				ITestWaitCondition<TSecond> second,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.first = first.CreateState();
				this.second = second.CreateState();
			}

			protected override async Task<TSecond> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				await this.first.Execute(runContext, cancellationToken);
				return await this.second.Execute(runContext, cancellationToken);
			}

			protected override void BuildDescription(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, new ContinuationState.Available(this.second));
		}
	}
}
