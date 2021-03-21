using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;

namespace Responsible.NoRx.TestInstructions
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
			private readonly Func<T1, ITestInstruction<T2>> selector;

			[CanBeNull] private ITestOperationState<T2> nextInstruction;

			public State(
				ITestInstruction<T1> first,
				Func<T1, ITestInstruction<T2>> selector,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.first = first.CreateState();
				this.selector = selector;
			}

			protected override async Task<T2> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
			{
				var firstResult = await this.first.Execute(runContext, cancellationToken);
				this.nextInstruction = this.selector(firstResult).CreateState();
				return await this.nextInstruction.Execute(runContext, cancellationToken);
			}

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, this.nextInstruction);
		}
	}
}
