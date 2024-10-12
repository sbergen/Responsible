using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestResponders
{
	internal class SelectResponder<T1, T2> : TestResponderBase<T2>
	{
		public SelectResponder(ITestResponder<T1> responder, Func<T1, T2> selector, SourceContext sourceContext)
			: base(() => new State(responder, selector, sourceContext))
		{
		}

		private class State : TestOperationState<ITestOperationState<T2>>
		{
			private readonly ITestOperationState<ITestOperationState<T1>> responder;
			private readonly Func<T1, T2> selector;
			private readonly SourceContext sourceContext;

			[CanBeNull] private SelectOperationState<T1, T2> selectState;

			public State(
				ITestResponder<T1> responder,
				Func<T1, T2> selector,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.responder = responder.CreateState();
				this.selector = selector;
				this.sourceContext = sourceContext;
			}

			protected override async Task<ITestOperationState<T2>> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				var instructionState = await this.responder.Execute(runContext, cancellationToken);
				this.selectState = new SelectOperationState<T1, T2>(
					instructionState, this.selector, this.sourceContext);
				return this.selectState;
			}

			protected override void BuildDescription(StateStringBuilder builder) =>
				builder.AddSelect<T1, T2>(
					this.responder,
					this.selectState?.SelectStatus ?? TestOperationStatus.NotExecuted.Instance);
		}
	}
}
