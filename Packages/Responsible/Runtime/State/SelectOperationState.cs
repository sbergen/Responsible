using System;
using Responsible.Context;
using UniRx;

namespace Responsible.State
{
	internal class SelectOperationState<T1, T2> : TestOperationState<T2>
	{
		private readonly ITestOperationState<T1> first;
		private readonly Func<T1, T2> selector;

		public SelectOperationState(ITestOperationState<T1> first, Func<T1, T2> selector, SourceContext? sourceContext)
			: base(sourceContext)
		{
			this.first = first;
			this.selector = selector;
		}

		protected override IObservable<T2> ExecuteInner(RunContext runContext) =>
			this.first
				.Execute(runContext)
				.Select(this.selector);

		public override void BuildDescription(StateStringBuilder builder) =>
			builder.AddSelect<T1, T2>(this.first, this);
	}
}