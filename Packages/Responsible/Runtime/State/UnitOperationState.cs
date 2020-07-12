using System;
using Responsible.Context;
using UniRx;

namespace Responsible.State
{
	/// <summary>
	/// Converts to unit-something. Difference to Select is that no source context is required,
	/// as this is assumed to be safe. The description is just forwarded.
	/// </summary>
	internal class UnitOperationState<T, TUnit> : TestOperationState<TUnit>
	{
		private readonly ITestOperationState<T> state;
		private readonly Func<T, TUnit> unitSelector;

		public UnitOperationState(ITestOperationState<T> state, Func<T, TUnit> unitSelector)
			: base(null)
		{
			this.state = state;
			this.unitSelector = unitSelector;
		}

		protected override IObservable<TUnit> ExecuteInner(RunContext runContext) => this.state
			.Execute(runContext)
			.Select(this.unitSelector);

		public override void BuildDescription(StateStringBuilder builder) => this.state.BuildDescription(builder);
	}
}