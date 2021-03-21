using System.Threading;
using System.Threading.Tasks;
using Responsible.NoRx.Context;

namespace Responsible.NoRx.State
{
	/// <summary>
	/// Converts to Nothing. Difference to Select is that no source context is required,
	/// as this is assumed to be safe. The description is just forwarded.
	/// </summary>
	internal class NothingOperationState<T> : TestOperationState<Nothing>
	{
		private readonly ITestOperationState<T> state;

		public NothingOperationState(ITestOperationState<T> state)
			: base(null)
		{
			this.state = state;
		}

		protected override async Task<Nothing> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
		{
			await this.state.Execute(runContext, cancellationToken);
			return Nothing.Default;
		}

		public override void BuildDescription(StateStringBuilder builder) => this.state.BuildDescription(builder);
	}
}
