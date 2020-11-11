using System.Collections.Generic;
using UnityEngine;

namespace ResponsibleGame
{
	public class TargetArea : MonoBehaviour
	{
		private readonly List<Marker> markers = new List<Marker>();

		[SerializeField] private RectTransform parentTransform = null;
		[SerializeField] private RectTransform rectTransform = null;
		[SerializeField] private Status status = null;

		[SerializeField] private Hit hitPrefab = null;
		[SerializeField] private Miss missPrefab = null;

		public void AddMarker(Vector2 canvasPosition)
		{
			var isHit = this.rectTransform.rect.Contains(canvasPosition);
			var prefab = isHit ? (Marker)this.hitPrefab : this.missPrefab;
			var marker = Instantiate(prefab, this.parentTransform);
			this.markers.Add(marker);
			marker.SetPosition(canvasPosition);
			this.status.HitOrMiss(isHit);
		}

		public void ClearMarkers()
		{
			foreach (var marker in this.markers)
			{
				Destroy(marker.gameObject);
			}

			this.markers.Clear();
		}
	}
}
