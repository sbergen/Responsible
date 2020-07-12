using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class SequencedTestInstruction<T1, T2> : TestInstructionBase<T2>
	{
		public SequencedTestInstruction(
			ITestInstruction<T1> first,
			ITestInstruction<T2> second,
			SourceContext sourceContext)
			: base(() => new State(first, second, sourceContext))
		{
		}

		private class State : TestOperationState<T2>
		{
			private readonly ITestOperationState<T1> first;
			private readonly ITestOperationState<T2> second;

			public State(
				ITestInstruction<T1> first,
				ITestInstruction<T2> second,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.first = first.CreateState();
				this.second = second.CreateState();
			}

			protected override IObservable<T2> ExecuteInner(RunContext runContext) => this.first
				.Execute(runContext)
				.ContinueWith(_ => this.second.Execute(runContext));

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, this.second);
		}
	}
}