using System;
using System.Collections.Generic;
using System.Linq;
using Responsible.Context;
using Responsible.Utilities;
using UniRx;

namespace Responsible.TestWaitConditions
{
	public class RespondToAllOfWaitCondition<T> : ITestWaitCondition<T[]>
	{
		private const string BaseDescription = "RESPOND TO ALL OF";
		private readonly IReadOnlyList<ITestResponder<T>> responders;

		public RespondToAllOfWaitCondition(IReadOnlyList<ITestResponder<T>> responders)
		{
			this.responders = responders;
		}

		public IObservable<T[]> WaitForResult(RunContext runContext, WaitContext waitContext) =>
			this.responders
				.Select((responder, i) => responder.InstructionWaitCondition
					.WaitForResult(runContext, waitContext)
					.WithIndex(i))
				.Merge() // Allow instructions to become ready in any order,
				.Select(indexedInstruction => indexedInstruction.Value
					.Run(runContext)
					.WithIndexFrom(indexedInstruction))
				.Concat() // ...but sequence execution of instruction
				.Aggregate(new T[this.responders.Count], Indexed.AssignToArray)
				.DoOnSubscribe(() => waitContext.MarkAsStarted(this))
				.Do(_ => waitContext.MarkAsCompleted(this));

		public void BuildDescription(ContextStringBuilder builder) =>
			builder.Add(BaseDescription, this.responders);

		public void BuildFailureContext(ContextStringBuilder builder) => builder.
			Add(
				builder.DescriptionForWait(this, BaseDescription),
				this.responders);
	}
}