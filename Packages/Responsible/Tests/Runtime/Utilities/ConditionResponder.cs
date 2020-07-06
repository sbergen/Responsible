using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime.Utilities
{
	public class ConditionResponder
	{
		public bool MayRespond { get; set; }
		public bool MayComplete { get; set; }

		public bool StartedToRespond { get; private set; }
		public bool CompletedRespond { get; private set; }

		public ITestResponder<Unit> Responder { get; }

		public ConditionResponder(int responseTimeout)
		{
			this.Responder = WaitForCondition("Wait to be ready", () => this.MayRespond)
				.ThenRespondWith("Respond",
					Do(() => this.StartedToRespond = true)
						.ContinueWith(
							WaitForCondition("Wait for completion", () => this.MayComplete)
								.ExpectWithinSeconds(responseTimeout)
								.ContinueWith(Do(() => this.CompletedRespond = true))))
				.AsUnitResponder();
		}

	}
}