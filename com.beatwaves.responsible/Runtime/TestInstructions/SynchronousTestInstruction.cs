using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestInstructions
{
	internal class SynchronousTestInstruction<T> : TestInstructionBase<T>
	{
		public SynchronousTestInstruction(string description, Func<T> action, SourceContext sourceContext)
		: base(() => new State(description, action, sourceContext))
		{
		}

		private class State : TestOperationState<T>
		{
			private readonly string description;
			private readonly Func<T> action;

			public State(string description, Func<T> action, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.description = description;
				this.action = action;
			}

			protected override Task<T> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
				=> Task.FromResult(this.action());

			protected override void BuildDescription(StateStringBuilder builder) =>
				builder.AddInstruction(this, this.description);
		}
	}
}
