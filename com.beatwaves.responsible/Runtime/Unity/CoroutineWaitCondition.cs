using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;
using Responsible.TestWaitConditions;
using Responsible.Utilities;
using UnityEngine;

namespace Responsible.Unity
{
	internal class CoroutineWaitCondition : TestWaitConditionBase<object>
	{
		public CoroutineWaitCondition(
			string description,
			Func<IEnumerator> startCoroutine,
			SourceContext sourceContext)
			: base(() => new State(description, startCoroutine, sourceContext))
		{
		}

		private class State : TestOperationState<object>, IDiscreteWaitConditionState
		{
			private readonly Func<IEnumerator> startCoroutine;

			public string Description { get; }
			public Action<StateStringBuilder> ExtraContext => null;

			public State(
				string description,
				Func<IEnumerator> startCoroutine,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.Description = $"{description} (Coroutine)";
				this.startCoroutine = startCoroutine;
			}

			protected override async Task<object> ExecuteInner(
				RunContext runContext, CancellationToken cancellationToken)
			{
				var completionSource = new TaskCompletionSource<object>();

				var unityTimeProvider = runContext.TimeProvider as MonoBehaviour;
				if (unityTimeProvider == null)
				{
					throw new Exception(
						"TimeProvider is not compatible with coroutines: " +
						$"Expected a {nameof(MonoBehaviour)}, got {runContext.TimeProvider.GetType()}!");
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
				TaskCompletionSource<object> completionSource,
				CancellationToken cancellationToken)
			{
				IEnumerator enumerator;

				try
				{
					enumerator = this.startCoroutine();
				}
				catch (Exception e)
				{
					completionSource.SetException(e);
					yield break;
				}

				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						if (!enumerator.MoveNext())
						{
							break;
						}
					}
					catch (Exception e)
					{
						completionSource.SetException(e);
						yield break;
					}

					yield return enumerator.Current;
				}

				HandleImpossibleConcurrentCancellation(cancellationToken, completionSource);
			}

			public override void BuildDescription(StateStringBuilder builder) => builder.AddWait(this);

			[ExcludeFromCodeCoverage]
			private static void HandleImpossibleConcurrentCancellation(
				CancellationToken cancellationToken,
				TaskCompletionSource<object> completionSource)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					// It should never be possible to reach this state in a single thread,
					// but better safe than sorry...
					completionSource.SetCanceled();
				}
				else
				{
					completionSource.SetResult(Unit.Instance);
				}
			}
		}
	}
}
