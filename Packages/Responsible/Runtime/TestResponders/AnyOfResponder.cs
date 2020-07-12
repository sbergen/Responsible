using System;
using System.Collections.Generic;
using System.Linq;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestResponders
{
	internal class AnyOfResponder<T> : OptionalTestResponderBase
	{
		public AnyOfResponder(IReadOnlyList<ITestResponder<T>> responders)
		: base(() => new State(responders))
		{
		}

		private class State : OperationState<IOperationState<Unit>>
		{
			private readonly IReadOnlyList<IOperationState<IOperationState<T>>> responders;

			public State(IReadOnlyList<ITestResponder<T>> responders)
			{
				this.responders = responders.Select(r => r.CreateState()).ToList();
			}


			protected override IObservable<IOperationState<Unit>> ExecuteInner(RunContext runContext) => this.responders
				.Select(responder => responder.Execute(runContext))
				.Merge()
				.Select(instruction => instruction.AsUnitOperationState());

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddParentWithChildren("RESPOND TO ANY OF", this, this.responders);
		}
	}
}