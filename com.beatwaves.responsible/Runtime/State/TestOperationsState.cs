using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;

namespace Responsible.State
{
	internal abstract class TestOperationState<T> : ITestOperationState<T>
	{
		private readonly SourceContext? sourceContext;
		private TestOperationStatus status = TestOperationStatus.NotExecuted.Instance;

		TestOperationStatus ITestOperationState.Status => this.status;

		protected TestOperationState(SourceContext? sourceContext)
		{
			this.sourceContext = sourceContext;
		}

		async Task<TResult> ITestOperationState<T>.ExecuteUnsafe<TResult>(RunContext runContext, CancellationToken cancellationToken)
		{
			var nestedRunContext = this.sourceContext != null
				? runContext.MakeNested(this.sourceContext.Value)
				: runContext;
			this.status = new TestOperationStatus.Waiting(this.status, runContext.MakeWaitContext());

			try
			{
				var result = await this.ExecuteInner(nestedRunContext, cancellationToken);
				this.status = new TestOperationStatus.Completed(this.status);
				return (TResult)(object)result; // See the Execute extension method for why this is safe.
			}
			catch (OperationCanceledException)
			{
				this.status = new TestOperationStatus.Canceled(this.status);
				throw;
			}
			catch (Exception e)
			{
				this.status = new TestOperationStatus.Failed(this.status, e, nestedRunContext.SourceContext);
				throw;
			}
		}

		protected abstract Task<T> ExecuteInner(RunContext runContext, CancellationToken cancellationToken);

		void ITestOperationState.BuildDescription(StateStringBuilder builder) => this.BuildDescription(builder);
		protected abstract void BuildDescription(StateStringBuilder builder);

		public override string ToString() => StateStringBuilder.MakeState(this);
	}
}
