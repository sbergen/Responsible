using UnityEngine;

namespace ResponsibleGame
{
	public class PlayerObject : MonoBehaviour
	{
		[SerializeField] private RectTransform rectTransform = null;
		[SerializeField] private RectTransform parentTransform = null;
		[SerializeField] private TargetArea targetArea = null;
		[SerializeField] private Status status = null;

		private bool goingLeft;

		private Vector2 CurrentPosition => this.rectTransform.localPosition;

		private void Awake()
		{
			PlayerInput.Instance.TriggerPressed += this.HandleTriggerPressed;
		}

		private void OnDestroy()
		{
			var inputInstance = PlayerInput.Instance;
			if (inputInstance != null)
			{
				inputInstance.TriggerPressed -= this.HandleTriggerPressed;
			}
		}

		private void HandleTriggerPressed()
		{
			if (this.status.IsAlive)
			{
				this.targetArea.AddMarker(this.CurrentPosition);
			}
			else
			{
				this.status.Restart();
				this.targetArea.ClearMarkers();
			}
		}

		void Update()
		{
			if (!this.status.IsAlive)
			{
				return;
			}

			// Quick-and-ugly "bouncing" of this object across the canvas.
			// Please don't consider this an example of the best way to do this :)

			var canvasWidth = this.parentTransform.rect.width;
			var rightBound = canvasWidth / 2;
			var leftBound = -rightBound;

			var direction = this.goingLeft ? -1 : 1;
			var currentX = this.CurrentPosition.x;
			var difficultyFactor = 1 + 0.1f * this.status.Score;
			var nextX = currentX + direction * canvasWidth * Time.deltaTime * difficultyFactor;

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

			var nextPosition = this.CurrentPosition;
			nextPosition[0] = nextX;
			this.rectTransform.localPosition = nextPosition;
		}
	}
}
