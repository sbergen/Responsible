using UnityEngine;

namespace Responsible.UnityTests.Utilities
{
	public class CoroutineRunner : MonoBehaviour
	{
		// Just used for starting coroutines in tests

		public static CoroutineRunner Create() =>
			new GameObject(nameof(CoroutineRunner)).AddComponent<CoroutineRunner>();

		public void Destroy() => DestroyImmediate(this.gameObject);
	}
}
