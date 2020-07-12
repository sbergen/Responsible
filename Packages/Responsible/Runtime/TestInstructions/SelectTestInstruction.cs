using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class SelectTestInstruction<T1, T2> : TestInstructionBase<T2>
	{
		public SelectTestInstruction(
			ITestInstruction<T1> first,
			Func<T1, T2> selector,
			SourceContext sourceContext)
			: base(() => new State(first, selector, sourceContext))
		{
		}

		private class State : OperationState<T2>
		{
			private readonly IOperationState<T1> first;
			private readonly Func<T1, T2> selector;

			public State(ITestInstruction<T1> first, Func<T1, T2> selector, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.first = first.CreateState();
				this.selector = selector;
			}

			protected override IObservable<T2> ExecuteInner(RunContext runContext) =>
				this.first.Execute(runContext).Select(this.selector);

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddInstruction(
					this,
					$"Select {typeof(T1).Name} -> {typeof(T2).Name}");
		}
	}
}