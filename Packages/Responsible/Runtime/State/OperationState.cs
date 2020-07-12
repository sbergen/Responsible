using System;
using Responsible.Context;
using UniRx;

namespace Responsible.State
{
	internal static class OperationState
	{
		public static IOperationState<Unit> AsUnitOperationState<T>(this IOperationState<T> state)
			=> new UnitOperationState<T>(state);
	}

	internal abstract class OperationState<T> : IOperationState<T>
	{
		private readonly SourceContext? sourceContext;

		public OperationStatus Status { get; private set; } = OperationStatus.NotExecuted.Instance;

		protected OperationState(SourceContext? sourceContext)
		{
			this.sourceContext = sourceContext;
		}

		public IObservable<T> Execute(RunContext runContext) => Observable.Defer(() =>
		{
			if (this.Status != OperationStatus.NotExecuted.Instance)
			{
				throw new InvalidOperationException("Operation already started");
			}

			var waitContext = runContext.MakeWaitContext();
			var nestedRunContext = this.sourceContext != null
				? runContext.MakeNested(this.sourceContext.Value)
				: runContext;
			this.Status = new OperationStatus.Waiting(this.Status, waitContext);

			return this
				.ExecuteInner(nestedRunContext)
				.DoOnCompleted(() => this.Status = new OperationStatus.Completed(this.Status))
				.DoOnError(exception => this.Status =
					new OperationStatus.Failed(this.Status, exception, nestedRunContext.SourceContext))
				.Finally(() => waitContext?.Dispose());
		});

		protected abstract IObservable<T> ExecuteInner(RunContext runContext);

		public abstract void BuildDescription(StateStringBuilder builder);

		public override string ToString() => StateStringBuilder.MakeState(this);
	}
}