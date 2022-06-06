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
		public RepeatedlyResponder(ITestResponder<T> respondTo, SourceContext sourceContext)
			: base(() => new State(respondTo, sourceContext))
		{
		}

		private sealed class State : TestOperationState<IMultipleTaskSource<ITestOperationState<object>>>
		{
			private readonly List<ITestOperationState> states = new List<ITestOperationState>();
			private readonly ITestResponder<T> respondTo;

			public State(ITestResponder<T> respondTo, SourceContext? sourceContext)
				: base(sourceContext)
			{
				this.respondTo = respondTo;
			}

			protected override Task<IMultipleTaskSource<ITestOperationState<object>>> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				return Task.FromResult<IMultipleTaskSource<ITestOperationState<object>>>(
					new RepeatedTaskSource<ITestOperationState<object>>(async ct =>
					{
						var state = this.respondTo.CreateState();
						this.states.Add(state);
						return (await state.Execute(runContext, ct)).BoxResult();
					}));

			}

			public override void BuildDescription(StateStringBuilder builder)
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
