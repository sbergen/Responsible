using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;

namespace Responsible.NoRx.TestWaitConditions
{
	internal class PollingWaitCondition<T> : TestWaitConditionBase<T>
	{
		public PollingWaitCondition(
			string description,
			Func<T> getConditionState,
			Func<T, bool> condition,
			[CanBeNull] Action<StateStringBuilder> extraContext,
			SourceContext sourceContext)
			: base(() => new State(description, getConditionState, condition, extraContext, sourceContext))
		{
		}

		private class State : TestOperationState<T>, IDiscreteWaitConditionState
		{
			private readonly Func<T> getConditionState;
			private readonly Func<T, bool> condition;

			public string Description { get; }
			public Action<StateStringBuilder> ExtraContext { get; }

			public State(
				string description,
				Func<T> getConditionState,
				Func<T, bool> condition,
				[CanBeNull] Action<StateStringBuilder> extraContext,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.Description = description;
				this.getConditionState = getConditionState;
				this.condition = condition;
				this.ExtraContext = extraContext;
			}

			protected override async Task<T> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
			{
				var tcs = new TaskCompletionSource<T>();

				void CheckCondition()
				{
					if (cancellationToken.IsCancellationRequested)
					{
						return;
					}

					try
					{
						var state = this.getConditionState();
						if (this.condition(state))
						{
							tcs.SetResult(state);
						}
					}
					catch (Exception e)
					{
						tcs.SetException(e);
					}
				}

				using (runContext.TimeProvider.RegisterPollCallback(CheckCondition))
				using (cancellationToken.Register(tcs.SetCanceled))
				{
					CheckCondition(); // Complete immediately if already met
					return await tcs.Task;
				}
			}

			public override void BuildDescription(StateStringBuilder builder) => builder.AddWait(this);
		}
	}
}
