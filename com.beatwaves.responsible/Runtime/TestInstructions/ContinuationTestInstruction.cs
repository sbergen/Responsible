using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestInstructions
{
	internal class ContinuationTestInstruction<T1, T2> : TestInstructionBase<T2>
	{
		public ContinuationTestInstruction(
			ITestInstruction<T1> first,
			Func<T1, ITestInstruction<T2>> selector,
			SourceContext sourceContext)
			: base(() => new State(first, selector, sourceContext))
		{
		}

		private class State : TestOperationState<T2>
		{
			private readonly ITestOperationState<T1> first;
			private readonly Continuation<T1, T2> continuation;

			public State(
				ITestInstruction<T1> first,
				Func<T1, ITestInstruction<T2>> selector,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.first = first.CreateState();
				this.continuation = new Continuation<T1, T2>(firstResult => selector(firstResult).CreateState());
			}

			protected override async Task<T2> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
			{
				var firstResult = await this.first.Execute(runContext, cancellationToken);
				return await this.continuation.Execute(firstResult, runContext, cancellationToken);
			}

			protected override void BuildDescription(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, this.continuation.State);
		}
	}
}
