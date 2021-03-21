using System;
using System.Threading;
using System.Threading.Tasks;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;

namespace Responsible.NoRx.TestInstructions
{
	internal class WaitForInstruction : TestInstructionBase<Nothing>
	{
		public WaitForInstruction(TimeSpan waitTime, SourceContext sourceContext)
		: base(() => new State(waitTime, sourceContext))
		{
		}

		private class State : TestOperationState<Nothing>
		{
			private readonly TimeSpan waitTime;

			public State(TimeSpan waitTime, SourceContext sourceContext)
				: base(sourceContext)
			{
				this.waitTime = waitTime;
			}

			protected override async Task<Nothing> ExecuteInner(
				RunContext runContext,
				CancellationToken cancellationToken)
			{
				var deadline = runContext.TimeProvider.TimeNow + this.waitTime;

				var tcs = new TaskCompletionSource<Nothing>();
				using (runContext.TimeProvider.RegisterPollCallback(() =>
				{
					if (cancellationToken.IsCancellationRequested)
					{
						tcs.SetCanceled();
					}
					else if (runContext.TimeProvider.TimeNow >= deadline)
					{
						tcs.SetResult(Nothing.Default);
					}
				}))
				{
					return await tcs.Task;
				}
			}

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddInstruction(this, $"WAIT FOR {this.waitTime:g}");
		}
	}
}
