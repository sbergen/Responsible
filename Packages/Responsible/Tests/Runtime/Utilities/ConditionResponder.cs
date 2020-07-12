using System;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime.Utilities
{
	public class ConditionResponder<T> : IConditionResponder
	{
		public const string WaitForCompletionDescription = "Wait for completion";

		private Exception exception;

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

		public ConditionResponder(int responseTimeout, T returnValue)
		{
			this.Responder = WaitForCondition("Wait to be ready", () => this.MayRespond)
				.ThenRespondWith(
					"Respond",
					Do("Set started to respond", () => this.StartedToRespond = true)
						.ContinueWith(this.WaitAndComplete(responseTimeout, returnValue)));

		}

		private ITestInstruction<T> WaitAndComplete(int responseTimeout, T returnValue) => WaitForCondition(
				WaitForCompletionDescription,
				() => this.MayComplete)
			.ExpectWithinSeconds(responseTimeout)
			.ContinueWith(Do(
				"Set complete or throw",
				() =>
				{
					this.CompletedRespond = true;
					return this.exception != null
						? throw this.exception
						: returnValue;
				}));
	}

	public class ConditionResponder : ConditionResponder<Unit>
	{
		public ConditionResponder(int responseTimeout)
			: base(responseTimeout, Unit.Default)
		{
		}
	}
}