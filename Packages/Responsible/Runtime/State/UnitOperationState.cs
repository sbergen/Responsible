using System;
using Responsible.Context;
using UniRx;

namespace Responsible.State
{
	/// <summary>
	/// Converts to unit-something. Difference to Select is that no source context is required,
	/// as this is assumed to be safe. The description is just forwarded.
	/// </summary>
	internal class UnitOperationState<T, TUnit> : IOperationState<TUnit>
	{
		private readonly IOperationState<T> state;
		private readonly Func<T, TUnit> unitSelector;

		public UnitOperationState(IOperationState<T> state, Func<T, TUnit> unitSelector)
		{
			this.state = state;
			this.unitSelector = unitSelector;
		}

		public OperationStatus Status => this.state.Status;
		public IObservable<TUnit> Execute(RunContext runContext) => this.state
			.Execute(runContext)
			.Select(this.unitSelector);

		public void BuildDescription(StateStringBuilder builder) => this.state.BuildDescription(builder);
	}
}