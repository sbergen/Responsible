using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;

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
			public ITestOperationState? InstructionState { get; private set; }

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

			protected override async Task<ITestOperationState<T>> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				var waitResult = await this.waitCondition.Execute(runContext, cancellationToken);
				var instruction = this.makeInstruction(waitResult).CreateState();
				this.InstructionState = instruction;
				return instruction;
			}

			public override void BuildDescription(StateStringBuilder builder) => builder.AddResponder(this);
		}
	}
}
