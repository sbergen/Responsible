using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestWaitConditions
{
	internal sealed class RepeatUntilCondition<TWait, TInstruction> : TestWaitConditionBase<TWait>
	{
		public RepeatUntilCondition(
			ITestInstruction<TInstruction> instruction,
			ITestWaitCondition<TWait> condition,
			int maximumRepeatCount,
			SourceContext sourceContext)
			: base(() => new State(instruction, condition, maximumRepeatCount, sourceContext))
		{
		}

		private sealed class State : TestOperationState<TWait>
		{
			private readonly List<ITestOperationState<TInstruction>> instructionStates = new List<ITestOperationState<TInstruction>>();
			private readonly ITestInstruction<TInstruction> instruction;
			private readonly int maximumRepeatCount;
			private readonly ITestOperationState<TWait> waitState;
			

			public State(
				ITestInstruction<TInstruction> instruction,
				ITestWaitCondition<TWait> condition,
				int maximumRepeatCount,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.instruction = instruction;
				this.maximumRepeatCount = maximumRepeatCount;
				this.waitState = condition.CreateState();
			}

			public override void BuildDescription(StateStringBuilder builder)
			{
				builder.AddNestedDetails("UNTIL", this.waitState.BuildDescription);
				builder.AddNestedDetails("REPEATEDLY EXECUTING", b =>
				{
					foreach (var state in this.instructionStates)
					{
						state.BuildDescription(b);
					}

					if (this.waitState.Status is TestOperationStatus.Waiting ||
						this.waitState.Status is TestOperationStatus.NotExecuted)
					{
						b.AddDetails("...");
					}
				});
			}

			protected override async Task<TWait> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
			{
				var executionCount = 0;
				var waitTask = this.waitState.Execute(runContext, cancellationToken);
				while (!waitTask.IsCanceled &&
					!waitTask.IsCompleted &&
					!waitTask.IsFaulted)
				{
					if (executionCount == maximumRepeatCount)
					{
						throw new RepetitionLimitExceededException(executionCount);
					}

					var state = this.instruction.CreateState();
					this.instructionStates.Add(state);
					await state.Execute(runContext, cancellationToken);
					++executionCount;
				}
				
				return await waitTask;
			}
		}
	}
}
