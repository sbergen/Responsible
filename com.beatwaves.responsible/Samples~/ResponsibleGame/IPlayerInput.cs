using System;

namespace ResponsibleGame
{
	public interface IPlayerInput
	{
		// Because Update execution order is really hard to predict,
		// use an event driven approach here, instead of polling from Update methods.
		event Action TriggerPressed;
	}
}
