using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;

namespace Responsible.State
{
	internal class BoxedOperationState<T, TResult> : TestOperationState<TResult>, IBoxedOperationState
		where TResult : class
	{
		private readonly ITestOperationState<T> state;
		private readonly Func<T, TResult> boxingSelector;

		public ITestOperationState WrappedState => this.state;

		public BoxedOperationState(ITestOperationState<T> state, Func<T, TResult> boxingSelector)
			: base(null)
		{
			this.state = state;
			this.boxingSelector = boxingSelector;
		}

		protected override async Task<TResult> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
		{
			var result = await this.state.Execute(runContext, cancellationToken);
			return this.boxingSelector(result);
		}

		public override void BuildDescription(StateStringBuilder builder) => this.state.BuildDescription(builder);
	}

	internal class BoxedOperationState<T> : BoxedOperationState<T, object>
	{
		private static readonly Func<T, object> BoxingSelector = value => value;

		public BoxedOperationState(ITestOperationState<T> state)
			: base(state, BoxingSelector)
		{
		}
	}
}
