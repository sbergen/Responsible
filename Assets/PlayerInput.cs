using UnityEngine;

public class PlayerInput : IPlayerInput
{
	// Proper dependency injection is out of scope for this project,
	// use globally mutable singleton instead...
	// See e.g. https://github.com/svermeulen/Extenject for how to do this properly.
	public static IPlayerInput Instance { get; set; }

	static PlayerInput()
	{
		Instance = new PlayerInput();
	}

	public bool TriggerPressed() => Input.GetKeyDown(KeyCode.Space);
}