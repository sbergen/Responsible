using System;
using Responsible.Context;
using UniRx;

namespace Responsible.State
{
	internal class UnitOperationState<T> : IOperationState<Unit>
	{
		private readonly IOperationState<T> state;

		public UnitOperationState(IOperationState<T> state)
		{
			this.state = state;
		}

		public OperationStatus Status => this.state.Status;
		public IObservable<Unit> Execute(RunContext runContext) => this.state.Execute(runContext).AsUnitObservable();
		public void BuildDescription(StateStringBuilder builder) => this.state.BuildDescription(builder);
	}
}