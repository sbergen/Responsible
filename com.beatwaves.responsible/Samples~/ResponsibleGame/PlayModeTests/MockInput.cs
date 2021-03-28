using System;

namespace ResponsibleGame.PlayModeTests
{
	public class MockInput : IPlayerInput
	{
		public void Trigger() => this.TriggerPressed?.Invoke();

		public event Action TriggerPressed;
	}
}
