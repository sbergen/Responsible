using System;
using Responsible.Utilities;
using UnityEngine;

namespace Responsible.Unity
{
	/// <summary>
	/// Default <see cref="ITimeProvider"/> implementation for Unity.
	/// Will call polling methods in <c>Update</c>.
	/// </summary>
	/// <remarks>
	/// If you want to customize this behaviour, it is recommended to just implement
	/// your own custom implementation, instead of reusing this one.
	/// However, if you want to compose a custom <see cref="TestInstructionExecutor"/>
	/// for Unity, using this as a time provider is recommended.
	/// </remarks>
	public class UnityTimeProvider : MonoBehaviour, ITimeProvider, IDisposable
	{
		private readonly RetryingPoller poller = new RetryingPoller();

		/// <summary>
		/// Creates a <see cref="UnityTimeProvider"/> in a DontDestroyOnLoad GameObject.
		/// The GameObject will be destroyed when this instance is disposed.
		/// </summary>
		/// <returns></returns>
		public static UnityTimeProvider Create()
		{
			var go = new GameObject();
			DontDestroyOnLoad(go);
			return go.AddComponent<UnityTimeProvider>();
		}

		/// <summary>
		/// Will destroy the GameObject this instance is running on.
		/// </summary>
		public void Dispose()
		{
			DestroyImmediate(this.gameObject);
		}

		int ITimeProvider.FrameNow => Time.frameCount;
		DateTimeOffset ITimeProvider.TimeNow => DateTimeOffset.Now;

		IDisposable ITimeProvider.RegisterPollCallback(Action action) =>
			this.poller.RegisterPollCallback(action);

		private void Update() => this.poller.Poll();
	}
}
