using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using Responsible.Unity;

namespace Responsible.TestWaitConditions
{
	internal class CoroutineWaitCondition : TestWaitConditionBase<Nothing>
	{
		public CoroutineWaitCondition(
			string description,
			Func<IEnumerator> startCoroutine,
			SourceContext sourceContext)
			: base(() => new State(description, startCoroutine, sourceContext))
		{
		}

		private class State : TestOperationState<Nothing>, IDiscreteWaitConditionState
		{
			private readonly Func<IEnumerator> startCoroutine;

			public string Description { get; }
			public Action<StateStringBuilder> ExtraContext => null;

			public State(
				[CanBeNull] string description,
				Func<IEnumerator> startCoroutine,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.Description = $"{description} (Coroutine)";
				this.startCoroutine = startCoroutine;
			}

			protected override async Task<Nothing> ExecuteInner(
				RunContext runContext, CancellationToken cancellationToken)
			{
				var completionSource = new TaskCompletionSource<Nothing>();

				// TODO, use some interface here
				var unityTimeProvider = runContext.TimeProvider as UnityTimeProvider;
				if (unityTimeProvider == null)
				{
					throw new Exception("TimeProvider is not compatible with coroutines!");
				}

				cancellationToken.ThrowIfCancellationRequested();
				var coroutine = unityTimeProvider.StartCoroutine(
					this.RunCoroutine(completionSource, cancellationToken));
				using (cancellationToken.Register(() => unityTimeProvider.StopCoroutine(coroutine)))
				{
					return await completionSource.Task;
				}
			}

			private IEnumerator RunCoroutine(
				TaskCompletionSource<Nothing> completionSource,
				CancellationToken cancellationToken)
			{
				var enumerator = this.startCoroutine();
				while (!cancellationToken.IsCancellationRequested && enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}

				completionSource.SetResult(Nothing.Default);
			}

			public override void BuildDescription(StateStringBuilder builder) => builder.AddWait(this);
		}
	}
}
