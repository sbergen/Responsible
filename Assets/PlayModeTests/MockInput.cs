using System;
using UniRx;

namespace PlayModeTests
{
	public class MockInput : IPlayerInput
	{
		private readonly Subject<Unit> triggerPresses = new Subject<Unit>();

		public void Trigger() => this.triggerPresses.OnNext(Unit.Default);

		public IObservable<Unit> TriggerPressed => this.triggerPresses;
	}
}