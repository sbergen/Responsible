using System;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestResponders
{
	internal class TestResponder<TArg, T> : TestResponderBase<T>
	{
		internal TestResponder(
			string description,
			ITestWaitCondition<TArg> waitCondition,
			Func<TArg, ITestInstruction<T>> makeInstruction,
			SourceContext sourceContext)
			: base(() => new State(description, waitCondition, makeInstruction, sourceContext))
		{
		}

		private class State : OperationState<IOperationState<T>>
		{
			private readonly string description;
			private readonly IOperationState<TArg> waitCondition;
			private readonly Func<TArg, ITestInstruction<T>> makeInstruction;

			[CanBeNull] private IOperationState<T> instructionState;

			public State(
				string description,
				ITestWaitCondition<TArg> waitCondition,
				Func<TArg, ITestInstruction<T>> makeInstruction,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.description = description;
				this.waitCondition = waitCondition.CreateState();
				this.makeInstruction = makeInstruction;
			}

			protected override IObservable<IOperationState<T>> ExecuteInner(RunContext runContext) =>
				this.waitCondition
					.Execute(runContext)
					.Select(arg => this.makeInstruction(arg).CreateState())
					.Do(instruction => this.instructionState = instruction);

			public override void BuildDescription(StateStringBuilder builder)
				=> builder.AddResponder(this.description, this, this.waitCondition, this.instructionState);
		}
	}
}