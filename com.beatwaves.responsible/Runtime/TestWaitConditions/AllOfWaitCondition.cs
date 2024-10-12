using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;
using Responsible.Utilities;

namespace Responsible.TestWaitConditions
{
	internal class AllOfWaitCondition<T> : TestWaitConditionBase<T[]>
	{
		public AllOfWaitCondition(IReadOnlyList<ITestWaitCondition<T>> conditions)
		: base (() => new State(conditions))
		{
		}

		private class State : TestOperationState<T[]>
		{
			private readonly IReadOnlyList<ITestOperationState<T>> conditions;

			public State(IReadOnlyList<ITestWaitCondition<T>> conditions)
				: base(null)
			{
				this.conditions = conditions.Select(c => c.CreateState()).ToList();
			}

			protected override async Task<T[]> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
			{
				using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
				{
					return await Task.WhenAll(this.conditions
						.Select(cond => cond
							.Execute(runContext, cancellationSource.Token)
							.CancelOnError(cancellationSource))
						.ToArray());
				}
			}


			protected override void BuildDescription(StateStringBuilder builder) =>
				builder.AddParentWithChildren(
					"WAIT FOR ALL OF",
					this,
					this.conditions);
		}
	}
}
