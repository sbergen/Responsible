using System;
using System.Collections;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class CoroutineWaitCondition<T> : TestWaitConditionBase<T>
	{
		public CoroutineWaitCondition(
			[CanBeNull] string description,
			Func<IEnumerator> startCoroutine,
			Func<T> makeResult,
			SourceContext sourceContext)
			: base(() => new State(description, startCoroutine, makeResult, sourceContext))
		{
		}

		private class State : TestOperationState<T>, IDiscreteWaitConditionState
		{
			private readonly Func<IEnumerator> startCoroutine;
			private readonly Func<T> makeResult;

			public string Description { get; }
			public Action<StateStringBuilder> ExtraContext => null;

			public State(
				[CanBeNull] string description,
				Func<IEnumerator> startCoroutine,
				Func<T> makeResult,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.Description = $"{description ?? startCoroutine.Method.Name} (Coroutine)";
				this.startCoroutine = startCoroutine;
				this.makeResult = makeResult;
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext) => Observable
				.FromCoroutine(this.startCoroutine)
				.Select(_ => this.makeResult());

			public override void BuildDescription(StateStringBuilder builder) => builder.AddWait(this);
		}
	}
}