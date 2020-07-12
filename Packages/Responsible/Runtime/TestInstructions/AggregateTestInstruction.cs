using System;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class AggregateTestInstruction<T1, T2> : TestInstructionBase<T2>
	{
		public AggregateTestInstruction(
			ITestInstruction<T1> first,
			Func<T1, ITestInstruction<T2>> selector,
			SourceContext sourceContext)
			: base(() => new State(first, selector, sourceContext))
		{
		}

		private class State : OperationState<T2>
		{
			private readonly IOperationState<T1> first;
			private readonly Func<T1, ITestInstruction<T2>> selector;

			[CanBeNull] private IOperationState<T2> nextInstruction;

			public State(
				ITestInstruction<T1> first,
				Func<T1, ITestInstruction<T2>> selector,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.first = first.CreateState();
				this.selector = selector;
			}

			protected override IObservable<T2> ExecuteInner(RunContext runContext) => this.first
				.Execute(runContext)
				.ContinueWith(result =>
				{
					this.nextInstruction = this.selector(result).CreateState();
					return this.nextInstruction.Execute(runContext);
				});

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, this.nextInstruction);
		}
	}
}