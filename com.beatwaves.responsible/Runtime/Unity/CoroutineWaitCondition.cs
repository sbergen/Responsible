using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
				[CanBeNull] string description,
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

				// TODO, use some interface here
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
				var enumerator = this.startCoroutine();
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

				if (cancellationToken.IsCancellationRequested)
				{
					completionSource.SetCanceled();
				}
				else
				{
					completionSource.SetResult(Unit.Instance);
				}
			}

			public override void BuildDescription(StateStringBuilder builder) => builder.AddWait(this);
		}
	}
}
