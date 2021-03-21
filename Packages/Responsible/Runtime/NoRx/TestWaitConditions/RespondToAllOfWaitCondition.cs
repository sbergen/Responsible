using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;
using Responsible.NoRx.Utilities;

namespace Responsible.NoRx.TestWaitConditions
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
				var instructionTasks = this.responders
					.Select((responder, i) => responder
						.Execute(runContext, cancellationToken)
						.WithIndex(i))
					.ToList();

				var results = new T[instructionTasks.Count];
				while (instructionTasks.Count > 0)
				{
					var readyToExecute = await Task.WhenAny(instructionTasks);
					instructionTasks.Remove(readyToExecute);

					var indexedInstruction = await readyToExecute;
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
