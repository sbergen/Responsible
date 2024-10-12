using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;

namespace Responsible.TestResponders
{
	internal class RepeatedlyResponder<T> : OptionalTestResponderBase
	{
		public RepeatedlyResponder(ITestResponder<T> respondTo, int maximumRepeatCount, SourceContext sourceContext)
			: base(() => new State(respondTo, maximumRepeatCount, sourceContext))
		{
		}

		private sealed class State : TestOperationState<IMultipleTaskSource<ITestOperationState<object>>>
		{
			private readonly List<ITestOperationState> states = new List<ITestOperationState>();
			private readonly ITestResponder<T> respondTo;
			private readonly int maximumRepeatCount;

			public State(
				ITestResponder<T> respondTo,
				int maximumRepeatCount,
				SourceContext? sourceContext)
				: base(sourceContext)
			{
				this.respondTo = respondTo;
				this.maximumRepeatCount = maximumRepeatCount;
			}

			protected override Task<IMultipleTaskSource<ITestOperationState<object>>> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				var repeatCount = 0;
				return Task.FromResult<IMultipleTaskSource<ITestOperationState<object>>>(
					new RepeatedTaskSource<ITestOperationState<object>>(async ct =>
					{
						var state = this.respondTo.CreateState();
						this.states.Add(state);
						var instruction = await state.Execute(runContext, ct);

						++repeatCount;
						if (repeatCount > this.maximumRepeatCount)
						{
							throw new RepetitionLimitExceededException(this.maximumRepeatCount);
						}

						return instruction.BoxResult();
					}));
			}

			protected override void BuildDescription(StateStringBuilder builder)
			{
				if (!this.states.Any())
				{
					builder.AddDetails("REPEATEDLY (responders never started)");
				}
				else
				{
					builder.AddToPreviousLineWithChildren(" REPEATEDLY", this.states);
				}
			}
		}
	}
}
