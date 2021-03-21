using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;
using Responsible.NoRx.Utilities;
using UnityEngine;

namespace Responsible.NoRx.TestResponders
{
	internal class AnyOfResponder<T> : OptionalTestResponderBase
	{
		public AnyOfResponder(IReadOnlyList<ITestResponder<T>> responders)
		: base(() => new State(responders))
		{
		}

		private class State : TestOperationState<IMultipleTaskSource<ITestOperationState<Nothing>>>
		{
			private readonly IReadOnlyList<ITestOperationState<ITestOperationState<T>>> responders;

			public State(IReadOnlyList<ITestResponder<T>> responders)
				: base(null)
			{
				this.responders = responders.Select(r => r.CreateState()).ToList();
			}

			protected override Task<IMultipleTaskSource<ITestOperationState<Nothing>>> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				Func<CancellationToken, Task<ITestOperationState<Nothing>>> MakeLauncher(
					ITestOperationState<ITestOperationState<T>> responder)
					=> async ct => (await responder.Execute(runContext, ct)).AsNothingOperationState();

				return Task.FromResult(MultipleTaskSource.Make(this.responders.Select(MakeLauncher)));
			}

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddToPreviousLineWithChildren(" ANY OF", this.responders);
		}
	}
}
