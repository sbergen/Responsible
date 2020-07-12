using System;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class SynchronousTestInstruction<T> : TestInstructionBase<T>
	{
		public SynchronousTestInstruction(string description, Func<T> action, SourceContext sourceContext)
		: base(() => new State(description, action, sourceContext))
		{
		}

		private class State : OperationState<T>
		{
			private readonly string description;
			private readonly Func<T> action;

			public State(string description, Func<T> action, SourceContext sourceContext)
			{
				this.description = description;
				this.action = action;
				this.SourceContext = sourceContext;
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext) => Observable.Defer(() =>
			{
				try
				{
					return Observable.Return(this.action());
				}
				catch (Exception e)
				{
					return Observable.Throw<T>(e);
				}
			});

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddInstruction(this, this.description);
		}
	}
}