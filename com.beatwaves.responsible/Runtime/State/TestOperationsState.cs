using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;

namespace Responsible.State
{
	internal abstract class TestOperationState<T> : ITestOperationState<T>
	{
		private readonly SourceContext? sourceContext;

		public TestOperationStatus Status { get; private set; } = TestOperationStatus.NotExecuted.Instance;

		protected TestOperationState(SourceContext? sourceContext)
		{
			this.sourceContext = sourceContext;
		}

		public async Task<TResult> ExecuteUnsafe<TResult>(RunContext runContext, CancellationToken cancellationToken)
		{
			// Defensive programming, should be impossible to trigger from the outside.
			// Stryker disable once statement
			TestOperationStatus.AssertNotStarted(this.Status);

			var nestedRunContext = this.sourceContext != null
				? runContext.MakeNested(this.sourceContext.Value)
				: runContext;
			this.Status = new TestOperationStatus.Waiting(this.Status, runContext.MakeWaitContext());

			try
			{
				var result = await this.ExecuteInner(nestedRunContext, cancellationToken);
				this.Status = new TestOperationStatus.Completed(this.Status);
				return (TResult)(object)result; // See the Execute extension method for why this is safe.
			}
			catch (OperationCanceledException)
			{
				this.Status = new TestOperationStatus.Canceled(this.Status);
				throw;
			}
			catch (Exception e)
			{
				this.Status = new TestOperationStatus.Failed(this.Status, e, nestedRunContext.SourceContext);
				throw;
			}
		}

		protected abstract Task<T> ExecuteInner(RunContext runContext, CancellationToken cancellationToken);

		public abstract void BuildDescription(StateStringBuilder builder);

		public override string ToString() => StateStringBuilder.MakeState(this);
	}
}
