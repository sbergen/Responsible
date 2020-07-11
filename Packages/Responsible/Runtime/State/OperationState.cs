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
		public OperationStatus Status { get; private set; } = OperationStatus.NotExecuted.Instance;

		public IObservable<T> Execute(RunContext runContext) => Observable.Defer(() =>
		{
			if (this.Status != OperationStatus.NotExecuted.Instance)
			{
				throw new InvalidOperationException("Operation already started");
			}

			return this.ExecuteInner(runContext)
				.DoOnSubscribe(() => this.Status = new OperationStatus.Waiting(this.Status, runContext))
				.Do(
					_ => this.Status = new OperationStatus.Completed(this.Status),
					e => this.Status = new OperationStatus.Failed(this.Status, e));
		});

		protected abstract IObservable<T> ExecuteInner(RunContext runContext);

		public abstract void BuildFailureContext(StateStringBuilder builder);
	}
}