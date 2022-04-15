using System;
using static Responsible.Responsibly;

namespace Responsible.Tests.Utilities
{
	public class ConditionResponder<T> : IConditionResponder
	{
		public const string WaitForCompletionDescription = "Wait for completion";

		private Exception exception;
		private Exception waitException;

		public bool MayRespond { get; set; }
		public bool MayComplete { get; set; }

		public bool StartedToRespond { get; private set; }
		public bool CompletedRespond { get; private set; }

		public ITestResponder<T> Responder { get; }

		public void AllowFullCompletion()
		{
			this.MayRespond = this.MayComplete = true;
		}

		public void AllowCompletionWithError(Exception e)
		{
			this.exception = e;
			this.AllowFullCompletion();
		}

		public void CompleteWaitWithError(Exception e)
		{
			this.waitException = e;
			this.AllowFullCompletion();
		}

		public ConditionResponder(int responseTimeout, T returnValue)
		{
			this.Responder = WaitForCondition("Wait to be ready", () =>
				{
					if (this.waitException != null)
					{
						throw this.waitException;
					}
					return this.MayRespond;
				})
				.ThenRespondWith(
					"Respond",
					DoAndReturn("Set started to respond", () => this.StartedToRespond = true)
						.ContinueWith(this.WaitAndComplete(responseTimeout, returnValue)));

		}

		private ITestInstruction<T> WaitAndComplete(int responseTimeout, T returnValue) => WaitForCondition(
				WaitForCompletionDescription,
				() => this.MayComplete)
			.ExpectWithinSeconds(responseTimeout)
			.ContinueWith(DoAndReturn(
				"Set complete or throw",
				() =>
				{
					this.CompletedRespond = true;
					return this.exception != null
						? throw this.exception
						: returnValue;
				}));
	}

	public class ConditionResponder : ConditionResponder<bool>
	{
		public ConditionResponder(int responseTimeout)
			: base(responseTimeout, true)
		{
		}
	}
}
