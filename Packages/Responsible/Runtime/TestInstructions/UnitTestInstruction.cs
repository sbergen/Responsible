using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	/// <summary>
	/// Provides "type erasure" for test instructions. We can't use Object as the generic type,
	/// as value types don't derive from it (no Any-type in C#)
	/// </summary>
	internal class UnitTestInstruction<T> : TestInstructionBase<Unit>
	{
		public UnitTestInstruction(ITestInstruction<T> instruction, SourceContext sourceContext)
			: base(() => new State(instruction, sourceContext))
		{
		}

		private class State : OperationState<Unit>
		{
			private readonly IOperationState<T> state;

			public State(ITestInstruction<T> instruction, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.state = instruction.CreateState();
			}

			protected override IObservable<Unit> ExecuteInner(RunContext runContext) =>
				this.state.Execute(runContext).AsUnitObservable();

			public override void BuildDescription(StateStringBuilder builder)
				=> this.state.BuildDescription(builder);
		}
	}
}