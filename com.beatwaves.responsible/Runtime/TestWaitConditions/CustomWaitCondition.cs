using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestWaitConditions
{
	internal sealed class CustomWaitCondition<T> : TestWaitConditionBase<T>
	{
		public CustomWaitCondition(
			ITestWaitCondition<T> condition,
			string description,
			SourceContext sourceContext)
			: base(() => new State(condition, description, sourceContext))
		{
		}

		private sealed class State : TestOperationState<T>, IDiscreteWaitConditionState
		{
			private readonly ITestOperationState<T> state;

			public string Description { get; }

			public Action<StateStringBuilder> ExtraContext => builder =>
			{
				if (this.Status is TestOperationStatus.Canceled or TestOperationStatus.Failed)
				{
					builder.AddNestedDetails(
						$"'{this.Description}' was built from:",
						this.state.BuildDescription);
				}
			};

			public State(
				ITestWaitCondition<T> condition,
				string description,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.Description = description;
				this.state = condition.CreateState();
			}

			protected override Task<T> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
				=> this.state.Execute(runContext, cancellationToken);

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddWait(this);
		}
	}
}
