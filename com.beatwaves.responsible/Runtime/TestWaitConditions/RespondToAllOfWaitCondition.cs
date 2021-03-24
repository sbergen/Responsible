using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;

namespace Responsible.TestWaitConditions
{
	internal class RespondToAllOfWaitCondition<T> : TestWaitConditionBase<T[]>
	{
		public RespondToAllOfWaitCondition(IReadOnlyList<ITestResponder<T>> responders)
			: base(() => new State(responders))
		{
		}

		private class State : TestOperationState<T[]>
		{
			private readonly IReadOnlyList<ITestOperationState<ITestOperationState<T>>> responders;

			public State(IReadOnlyList<ITestResponder<T>> responders)
				: base(null)
			{
				this.responders = responders.Select(r => r.CreateState()).ToList();
			}

			protected override async Task<T[]> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
			{
				var instructionAwaiter = MultipleTaskAwaiter.Make(this.responders
					.Select((responder, i) => responder
						.Execute(runContext, cancellationToken)
						.WithIndex(i)));

				var results = new T[this.responders.Count];
				while (instructionAwaiter.HasNext)
				{
					cancellationToken.ThrowIfCancellationRequested();
					var indexedInstruction = await instructionAwaiter.AwaitNext();
					var result = await indexedInstruction.Value
						.Execute(runContext, cancellationToken)
						.WithIndexFrom(indexedInstruction);

					Indexed.AssignToArray(results, result);
				}

				return results;
			}

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddToPreviousLineWithChildren(" ALL OF", this.responders);
		}
	}
}
