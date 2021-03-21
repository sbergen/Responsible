using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.NoRx.Context;

namespace Responsible.NoRx.State
{
	/// <summary>
	/// Converts to Nothing. Difference to Select is that no source context is required,
	/// as this is assumed to be safe. The description is just forwarded.
	/// </summary>
	internal class NothingOperationState<T, TNothing> : TestOperationState<TNothing>
	{
		private readonly ITestOperationState<T> state;
		private readonly Func<T, TNothing> nothingSelector;

		public NothingOperationState(ITestOperationState<T> state, Func<T, TNothing> nothingSelector)
			: base(null)
		{
			this.state = state;
			this.nothingSelector = nothingSelector;
		}

		protected override async Task<TNothing> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
		{
			var result = await this.state.Execute(runContext, cancellationToken);
			return this.nothingSelector(result);
		}

		public override void BuildDescription(StateStringBuilder builder) => this.state.BuildDescription(builder);
	}
}
