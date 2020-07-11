using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestResponders
{
	internal class UnitTestResponder<T> : TestResponderBase<Unit>
	{
		public UnitTestResponder(ITestResponder<T> responder)
		: base(() => new State(responder))
		{
		}

		private class State : OperationState<IOperationState<Unit>>
		{
			private readonly IOperationState<IOperationState<T>> responder;

			public State(ITestResponder<T> responder)
			{
				this.responder = responder.CreateState();
			}

			protected override IObservable<IOperationState<Unit>> ExecuteInner(RunContext runContext) =>
				this.responder.Execute(runContext).Select(state => state.AsUnitOperationState());

			public override void BuildFailureContext(StateStringBuilder builder) =>
				this.responder.BuildFailureContext(builder);
		}
	}
}