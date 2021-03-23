using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;

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

		protected override async Task<T2> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
		{
			var result = await this.first.Execute(runContext, cancellationToken);
			this.selectState = new SynchronousTestInstruction<T2>(
				"Internal select operation",
				() => this.selector(result),
				this.sourceContext).CreateState();
			return await this.selectState.Execute(runContext, cancellationToken);
		}

		public override void BuildDescription(StateStringBuilder builder) =>
			builder.AddSelect<T1, T2>(this.first, this.SelectStatus);
	}
}
