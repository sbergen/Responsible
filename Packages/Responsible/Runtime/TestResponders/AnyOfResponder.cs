using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;

namespace Responsible.TestResponders
{
	internal class AnyOfResponder<T> : OptionalTestResponderBase
	{
		public AnyOfResponder(IReadOnlyList<ITestResponder<T>> responders)
		: base(() => new State(responders))
		{
		}

		private class State : TestOperationState<IMultipleTaskSource<ITestOperationState<object>>>
		{
			private readonly IReadOnlyList<ITestOperationState<ITestOperationState<T>>> responders;

			public State(IReadOnlyList<ITestResponder<T>> responders)
				: base(null)
			{
				this.responders = responders.Select(r => r.CreateState()).ToList();
			}

			protected override Task<IMultipleTaskSource<ITestOperationState<object>>> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				DeferredTask<ITestOperationState<object>> MakeLauncher(
					ITestOperationState<ITestOperationState<T>> responder)
					=> async ct => (await responder.Execute(runContext, ct)).BoxResult();

				return Task.FromResult(MultipleTaskSource.Make(this.responders.Select(MakeLauncher)));
			}

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddToPreviousLineWithChildren(" ANY OF", this.responders);
		}
	}
}
