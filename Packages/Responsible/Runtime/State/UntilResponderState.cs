using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.Utilities;

namespace Responsible.State
{
	internal class UntilResponderState<T> : TestOperationState<T>
	{
		private readonly string untilDescription;
		private readonly ITestOperationState<IMultipleTaskSource<ITestOperationState<Nothing>>> respondTo;
		private readonly ITestOperationState<T> until;

		public UntilResponderState(
			string untilDescription,
			ITestOperationState<IMultipleTaskSource<ITestOperationState<Nothing>>> respondTo,
			ITestOperationState<T> until,
			SourceContext sourceContext)
			: base(sourceContext)
		{
			this.untilDescription = untilDescription;
			this.respondTo = respondTo;
			this.until = until;
		}

		protected override async Task<T> ExecuteInner(RunContext runContext, CancellationToken cancellationToken)
		{
			using (var respondTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
			{
				var untilTask = this.until
					.Execute(runContext, cancellationToken)
					.CancelOnCompletion(respondTokenSource, cancellationToken);

				var responsesSource = await this.respondTo.Execute(runContext, respondTokenSource.Token);
				var responses = responsesSource.Start(respondTokenSource.Token);

				while (responses.HasNext)
				{
					try
					{
						var responseInstruction = await responses.AwaitNext();
						await responseInstruction.Execute(runContext, cancellationToken);
					}
					catch (OperationCanceledException)
					{
						break;
					}
				}

				return await untilTask;
			}
		}

		public override void BuildDescription(StateStringBuilder builder) =>
			builder.AddUntilResponder(
				"RESPOND TO",
				this.respondTo,
				this.untilDescription,
				this.until);
	}
}
