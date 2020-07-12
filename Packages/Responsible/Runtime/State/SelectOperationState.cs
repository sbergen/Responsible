using System;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using UniRx;

namespace Responsible.State
{
	internal class SelectOperationState<T1, T2> : TestOperationState<T2>
	{
		private readonly ITestOperationState<T1> first;
		private readonly Func<T1, T2> selector;
		private readonly SourceContext sourceContext;

		[CanBeNull] private ITestOperationState<T2> selectState;

		public TestOperationStatus SelectStatus => this.selectState?.Status ?? TestOperationStatus.NotExecuted.Instance;

		public SelectOperationState(ITestOperationState<T1> first, Func<T1, T2> selector, SourceContext sourceContext)
			: base(sourceContext)
		{
			this.first = first;
			this.selector = selector;
			this.sourceContext = sourceContext;
		}

		protected override IObservable<T2> ExecuteInner(RunContext runContext) =>
			this.first
				.Execute(runContext)
				.Select(result => new SynchronousTestInstruction<T2>(
					"Internal select operation",
					() => this.selector(result),
					this.sourceContext).CreateState())
				.Do(state => this.selectState = state)
				.ContinueWith(state => state.Execute(runContext));

		public override void BuildDescription(StateStringBuilder builder) =>
			builder.AddSelect<T1, T2>(this.first, this.SelectStatus);
	}
}