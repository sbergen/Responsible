using System;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class ConstraintWaitCondition<T> : TestWaitConditionBase<T>
	{
		public ConstraintWaitCondition(
			string objectDescription,
			Func<T> getObject,
			IResolveConstraint constraint,
			SourceContext sourceContext)
			// Resolve() MUST be called only once!
			: this(objectDescription, getObject, constraint.Resolve(), sourceContext)
		{
		}

		private ConstraintWaitCondition(
			string objectDescription,
			Func<T> getObject,
			IConstraint constraint,
			SourceContext sourceContext)
			: base(() => new State(objectDescription, getObject, constraint, sourceContext))
		{
		}

		private class State : TestOperationState<T>, IDiscreteWaitConditionState
		{
			private readonly Func<T> getObject;
			private readonly IConstraint constraint;

			public string Description { get; }
			public Action<StateStringBuilder> ExtraContext => this.AddDetails;

			public State(
				string objectDescription,
				Func<T> getObject,
				IConstraint constraint,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.getObject = getObject;
				this.constraint = constraint;
				this.Description = $"{objectDescription}: {this.constraint.Description}";
			}

			protected override IObservable<T> ExecuteInner(RunContext runContext) => runContext
				.PollObservable
				.StartWith(Unit.Default) // Allow immediate completion
				.Select(_ => this.GetResult().IsSuccess)
				.Where(fulfilled => fulfilled)
				.Take(1)
				.Select(_ => this.getObject());

			public override void BuildDescription(StateStringBuilder builder) => builder.AddWait(this);

			private ConstraintResult GetResult() =>
				this.constraint.ApplyTo(this.getObject());

			private void AddDetails(StateStringBuilder stateBuilder)
			{
				try
				{
					var messageWriter = new TextMessageWriter();
					this.GetResult().WriteMessageTo(messageWriter);
					stateBuilder.AddDetails(messageWriter.ToString());
				}
				catch (Exception e)
				{
					stateBuilder.AddDetails($"Failed to get description:\n{e}");
				}
			}
		}
	}
}