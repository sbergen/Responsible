using System;
using System.Collections.Generic;
using System.Linq;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class RespondToAllOfWaitCondition<T> : TestWaitConditionBase<T[]>
	{
		public RespondToAllOfWaitCondition(IReadOnlyList<ITestResponder<T>> responders)
			: base(() => new State(responders))
		{
		}

		private class State : OperationState<T[]>
		{
			private readonly IReadOnlyList<IOperationState<IOperationState<T>>> responders;

			public State(IReadOnlyList<ITestResponder<T>> responders)
			{
				this.responders = responders.Select(r => r.CreateState()).ToList();
			}

			protected override IObservable<T[]> ExecuteInner(RunContext runContext) =>
				this.responders
					.Select((responder, i) => responder
						.Execute(runContext)
						.WithIndex(i))
					.Merge() // Allow instructions to become ready in any order,
					.Select(indexedInstruction => indexedInstruction.Value
						.Execute(runContext)
						.WithIndexFrom(indexedInstruction))
					.Concat() // ...but sequence execution of instruction
					.Aggregate(new T[this.responders.Count], Indexed.AssignToArray);

			public override void BuildFailureContext(StateStringBuilder builder) =>
				builder.AddParentWithChildren("RESPOND TO ALL OF", this, this.responders);
		}
	}
}