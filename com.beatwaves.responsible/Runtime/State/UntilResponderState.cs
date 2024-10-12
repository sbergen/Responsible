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
		private readonly ITestOperationState<IMultipleTaskSource<ITestOperationState<object>>> respondTo;
		private readonly ITestOperationState<T> until;

		public UntilResponderState(
			string untilDescription,
			ITestOperationState<IMultipleTaskSource<ITestOperationState<object>>> respondTo,
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
					.CancelOnTermination(respondTokenSource);

				var responsesSource = await this.respondTo.Execute(runContext, respondTokenSource.Token);
				var responses = responsesSource.Start(respondTokenSource.Token);

				try
				{
					while (responses.HasNext)
					{
						var responseInstruction = await responses.AwaitNext();
						await responseInstruction.Execute(runContext, cancellationToken);
					}
				}
				catch (OperationCanceledException)
				{
					// Don't throw for cancellation of waiting for responses
				}

				return await untilTask;
			}
		}

		protected override void BuildDescription(StateStringBuilder builder) =>
			builder.AddUntilResponder(
				"RESPOND TO",
				this.respondTo,
				this.untilDescription,
				this.until);
	}
}
