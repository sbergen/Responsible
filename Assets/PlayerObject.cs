using UnityEngine;

public class PlayerObject : MonoBehaviour
{
	[SerializeField] private RectTransform rectTransform = null;
	[SerializeField] private RectTransform parentTransform = null;
	[SerializeField] private TargetArea targetArea = null;
	[SerializeField] private Status status = null;

	private bool goingLeft;

	void Update()
	{
		if (!this.status.IsAlive)
		{
			if (PlayerInput.Instance.TriggerPressed())
			{
				this.status.Restart();
				this.targetArea.ClearMarkers();
			}
			return;
		}

		var currentPosition = this.rectTransform.localPosition;

		if (PlayerInput.Instance.TriggerPressed())
		{
			this.targetArea.AddMarker(currentPosition);
		}

		// Quick-and-ugly "bouncing" of this object across the canvas.
		// Please don't consider this an example of the best way to do this :)

		var canvasWidth = this.parentTransform.rect.width;
		var rightBound = canvasWidth / 2;
		var leftBound = -rightBound;

		var direction = this.goingLeft ? -1 : 1;
		var currentX = currentPosition.x;
		var nextX = currentX + direction * canvasWidth * Time.deltaTime;

		if (nextX > rightBound)
		{
			this.goingLeft = true;
			nextX = 2 * rightBound - nextX;
		}
		else if (nextX < leftBound)
		{
			this.goingLeft = false;
			nextX = 2 * leftBound - nextX;
		}

		var nextPosition = currentPosition;
		nextPosition[0] = nextX;
		this.rectTransform.localPosition = nextPosition;
	}
}
