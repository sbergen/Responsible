using System;
using System.Collections;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class CoroutineWaitCondition : TestWaitConditionBase<Unit>
	{
		public CoroutineWaitCondition(
			string description,
			Func<IEnumerator> startCoroutine,
			SourceContext sourceContext)
			: base(() => new State(description, startCoroutine, sourceContext))
		{
		}

		private class State : TestOperationState<Unit>, IDiscreteWaitConditionState
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

			protected override IObservable<Unit> ExecuteInner(RunContext runContext) => Observable
				.FromCoroutine(this.startCoroutine);

			public override void BuildDescription(StateStringBuilder builder) => builder.AddWait(this);
		}
	}
}