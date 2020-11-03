using UnityEngine;

public class Marker : MonoBehaviour
{
	[SerializeField] private RectTransform rectTransform = null;

	public void SetPosition(Vector2 position)
	{
		this.rectTransform.localPosition = position;
	}
}