using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime.Utilities
{
	public class ConditionResponder<T> : IConditionResponder
	{
		public bool MayRespond { get; set; }
		public bool MayComplete { get; set; }

		public bool StartedToRespond { get; private set; }
		public bool CompletedRespond { get; private set; }

		public ITestResponder<T> Responder { get; }

		public void AllowFullCompletion()
		{
			this.MayRespond = this.MayComplete = true;
		}

		public ConditionResponder(int responseTimeout, T returnValue)
		{
			this.Responder = WaitForCondition("Wait to be ready", () => this.MayRespond)
				.ThenRespondWith("Respond",
					Do(() => this.StartedToRespond = true)
						.ContinueWith(
							WaitForCondition("Wait for completion", () => this.MayComplete)
								.ExpectWithinSeconds(responseTimeout)
								.ContinueWith(Do(() =>
								{
									this.CompletedRespond = true;
									return returnValue;
								}))));
		}
	}

	public class ConditionResponder : ConditionResponder<Unit>
	{
		public ConditionResponder(int responseTimeout)
			: base(responseTimeout, Unit.Default)
		{
		}
	}
}