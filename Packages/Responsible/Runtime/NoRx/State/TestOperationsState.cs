using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.NoRx.Context;

namespace Responsible.NoRx.State
{
	internal abstract class TestOperationState<T> : ITestOperationState<T>
	{
		private readonly SourceContext? sourceContext;

		public TestOperationStatus Status { get; private set; } = TestOperationStatus.NotExecuted.Instance;

		protected TestOperationState(SourceContext? sourceContext)
		{
			this.sourceContext = sourceContext;
		}

		public async Task<T> Execute(RunContext runContext, CancellationToken cancellationToken)
		{
			TestOperationStatus.AssertNotStarted(this.Status);

			using var waitContext = runContext.MakeWaitContext();
			var nestedRunContext = this.sourceContext != null
				? runContext.MakeNested(this.sourceContext.Value)
				: runContext;
			this.Status = new TestOperationStatus.Waiting(this.Status, waitContext);

			try
			{
				var result = await this.ExecuteInner(nestedRunContext, cancellationToken);
				this.Status = new TestOperationStatus.Completed(this.Status);
				return result;
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
