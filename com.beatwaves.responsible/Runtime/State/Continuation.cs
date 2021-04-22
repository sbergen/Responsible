using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.Context;

namespace Responsible.State
{
	internal class Continuation<TFirst, TSecond>
	{
		private readonly Func<TFirst, ITestOperationState<TSecond>> makeContinuation;

		private TestOperationStatus creationStatus = TestOperationStatus.NotExecuted.Instance;
		[CanBeNull] private ITestOperationState<TSecond> executionState;

		public ContinuationState State => this.executionState != null
			? (ContinuationState)new ContinuationState.Available(this.executionState)
			: new ContinuationState.NotAvailable(this.creationStatus);

		public Continuation(Func<TFirst, ITestOperationState<TSecond>> makeContinuation)
		{
			this.makeContinuation = makeContinuation;
		}

		public async Task<TSecond> Execute(
			TFirst source,
			RunContext runContext,
			CancellationToken cancellationToken)
		{
			this.creationStatus =
				new TestOperationStatus.Waiting(this.creationStatus, runContext.MakeWaitContext());

			try
			{
				this.executionState = this.makeContinuation(source);
				this.creationStatus = new TestOperationStatus.Completed(this.creationStatus);
				return await this.executionState.Execute(runContext, cancellationToken);
			}
			catch (Exception e)
			{
				this.creationStatus =
					new TestOperationStatus.Failed(this.creationStatus, e, runContext.SourceContext);
				throw;
			}
		}
	}
}
