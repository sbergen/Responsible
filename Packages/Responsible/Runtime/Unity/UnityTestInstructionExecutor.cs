using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Responsible.Unity
{
	public class UnityTestInstructionExecutor : TestInstructionExecutor, IDisposable
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

		public void Dispose()
		{
			Object.DestroyImmediate(this.timeProvider.gameObject);
		}

		private static UnityTimeProvider CreateTimeProvider()
		{
			var go = new GameObject();
			Object.DontDestroyOnLoad(go);
			return go.AddComponent<UnityTimeProvider>();
		}
	}
}
