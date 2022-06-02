using System;
using System.Threading;
using System.Threading.Tasks;

namespace Responsible.Utilities
{
	internal class RunLoopScheduler : ITestScheduler
	{
		private readonly RunLoopExceptionSource runLoopExceptionSource = new RunLoopExceptionSource();
		private readonly RetryingPoller poller = new RetryingPoller();
		private int frameNow;

		public IExternalResultSource ExternalResultSource => this.runLoopExceptionSource;

		public void Run(Action runOneIteration)
		{
			try
			{
				runOneIteration();
				this.poller.Poll();
				++this.frameNow;
			}
			catch (Exception e)
			{
				this.runLoopExceptionSource.SetException(e);
			}
		}

		int ITestScheduler.FrameNow => this.frameNow;
		DateTimeOffset ITestScheduler.TimeNow => DateTimeOffset.Now;

		IDisposable ITestScheduler.RegisterPollCallback(Action action) =>
			this.poller.RegisterPollCallback(action);

		private class RunLoopExceptionSource : IExternalResultSource
		{
			private Action<Exception> abortCurrentInstruction;

			public void SetException(Exception exception)
			{
				// This is essentially guaranteed to not be null, so don't check it explicitly.
				this.abortCurrentInstruction(exception);
			}

			Task<T> IExternalResultSource.GetExternalResult<T>(CancellationToken cancellationToken)
			{
				var completionSource = new TaskCompletionSource<T>(cancellationToken);
				this.abortCurrentInstruction = completionSource.SetException;
				return completionSource.Task;
			}
		}
	}
}
