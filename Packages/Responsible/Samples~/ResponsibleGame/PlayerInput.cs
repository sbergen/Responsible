using System;
using UniRx;
using UnityEngine;

namespace ResponsibleGame
{
	public class PlayerInput : MonoBehaviour, IPlayerInput
	{
		private readonly Subject<Unit> triggerPressed = new Subject<Unit>();

		// Proper dependency injection is out of scope for this project,
		// use globally mutable singleton instead...
		// See e.g. https://github.com/svermeulen/Extenject for how to do this properly.
		public static IPlayerInput Instance { get; set; }

		IObservable<Unit> IPlayerInput.TriggerPressed => this.triggerPressed;

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
		}

		private void OnDestroy()
		{
			if (ReferenceEquals(this, Instance))
			{
				Instance = null;
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				this.triggerPressed.OnNext(Unit.Default);
			}
		}
	}
}