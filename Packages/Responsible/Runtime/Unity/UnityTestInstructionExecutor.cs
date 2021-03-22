using UnityEngine;
using Object = UnityEngine.Object;

namespace Responsible.Unity
{
	public class UnityTestInstructionExecutor : TestInstructionExecutor
	{
		private readonly UnityTimeProvider timeProvider;

		private UnityTestInstructionExecutor(UnityTimeProvider timeProvider)
			: base(timeProvider)
		{
			this.timeProvider = timeProvider;
		}

		public UnityTestInstructionExecutor()
			: this(CreateTimeProvider())
		{
		}

		public override void Dispose()
		{
			Object.DestroyImmediate(this.timeProvider.gameObject);
			base.Dispose();
		}

		private static UnityTimeProvider CreateTimeProvider()
		{
			var go = new GameObject();
			Object.DontDestroyOnLoad(go);
			return go.AddComponent<UnityTimeProvider>();
		}
	}
}
