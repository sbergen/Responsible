using System;
using UnityEngine;

namespace ResponsibleGame
{
	public class PlayerInput : MonoBehaviour, IPlayerInput
	{
		// Proper dependency injection is out of scope for this project,
		// use globally mutable singleton instead...
		// See e.g. https://github.com/svermeulen/Extenject for how to do this properly.
		public static IPlayerInput Instance { get; set; }

		public event Action TriggerPressed;

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
				this.TriggerPressed?.Invoke();
			}
		}
	}
}
