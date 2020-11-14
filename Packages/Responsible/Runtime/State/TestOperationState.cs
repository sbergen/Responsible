using System;
using Responsible.Context;
using UniRx;

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

		public IObservable<T> Execute(RunContext runContext) => Observable.Defer(() =>
		{
			TestOperationStatus.AssertNotStarted(this.Status);

			var waitContext = runContext.MakeWaitContext();
			var nestedRunContext = this.sourceContext != null
				? runContext.MakeNested(this.sourceContext.Value)
				: runContext;
			this.Status = new TestOperationStatus.Waiting(this.Status, waitContext);

			return this
				.ExecuteInner(nestedRunContext)
				.DoOnCompleted(() => this.Status = new TestOperationStatus.Completed(this.Status))
				.DoOnError(exception => this.Status =
					new TestOperationStatus.Failed(this.Status, exception, nestedRunContext.SourceContext))
				.Finally(() => waitContext.Dispose());
		});

		protected abstract IObservable<T> ExecuteInner(RunContext runContext);

		public abstract void BuildDescription(StateStringBuilder builder);

		public override string ToString() => StateStringBuilder.MakeState(this);
	}
}