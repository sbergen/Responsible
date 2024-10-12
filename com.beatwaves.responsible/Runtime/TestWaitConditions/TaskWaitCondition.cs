using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestWaitConditions
{
	internal class TaskWaitCondition<T> : TestWaitConditionBase<T>
	{
		public TaskWaitCondition(
			string description,
			Func<CancellationToken, Task<T>> taskRunner,
			SourceContext sourceContext)
			: base(() => new State(description, taskRunner, sourceContext))
		{
		}

		private class State : TestOperationState<T>, IDiscreteWaitConditionState
		{
			private readonly Func<CancellationToken, Task<T>> taskRunner;

			public string Description { get; }
			public Action<StateStringBuilder> ExtraContext => null;

			public State(string description, Func<CancellationToken, Task<T>> taskRunner, SourceContext sourceContext)
				: base(sourceContext)
				=> (this.Description, this.taskRunner) = (description, taskRunner);

			protected override Task<T> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
				=> this.taskRunner(cancellationToken);

			protected override void BuildDescription(StateStringBuilder builder)
				=> builder.AddWait(this);
		}
	}
}
