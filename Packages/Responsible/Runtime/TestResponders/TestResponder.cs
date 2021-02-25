using System;
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

		private class State : TestOperationState<ITestOperationState<T>>, IBasicResponderState
		{
			private readonly ITestOperationState<TArg> waitCondition;
			private readonly Func<TArg, ITestInstruction<T>> makeInstruction;

			public string Description { get; }
			public ITestOperationState WaitState => this.waitCondition;
			public ITestOperationState InstructionState { get; private set; }

			public State(
				string description,
				ITestWaitCondition<TArg> waitCondition,
				Func<TArg, ITestInstruction<T>> makeInstruction,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.Description = description;
				this.waitCondition = waitCondition.CreateState();
				this.makeInstruction = makeInstruction;
			}

			protected override IObservable<ITestOperationState<T>> ExecuteInner(RunContext runContext) =>
				this.waitCondition
					.Execute(runContext)
					.Select(arg => this.makeInstruction(arg).CreateState())
					.Do(instruction => this.InstructionState = instruction);

			public override void BuildDescription(StateStringBuilder builder) => builder.AddResponder(this);
		}
	}
}