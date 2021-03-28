using System;
using Responsible.Utilities;
using UnityEngine;

namespace Responsible.Unity
{
	/// <summary>
	/// Default <see cref="ITestScheduler"/> implementation for Unity.
	/// Will call polling methods in <c>Update</c>.
	/// </summary>
	/// <remarks>
	/// If you want to customize this behaviour, it is recommended to just implement
	/// your own custom implementation, instead of reusing this one.
	/// However, if you want to compose a custom <see cref="TestInstructionExecutor"/>
	/// for Unity, using this as a scheduler is recommended.
	/// </remarks>
	public class UnityTestScheduler : MonoBehaviour, ITestScheduler, IDisposable
	{
		private readonly RetryingPoller poller = new RetryingPoller();

		/// <summary>
		/// Creates a <see cref="UnityTestScheduler"/> in a DontDestroyOnLoad GameObject.
		/// The GameObject will be destroyed when this instance is disposed.
		/// </summary>
		/// <returns></returns>
		public static UnityTestScheduler Create()
		{
			var go = new GameObject();
			DontDestroyOnLoad(go);
			return go.AddComponent<UnityTestScheduler>();
		}

		/// <summary>
		/// Will destroy the GameObject this instance is running on.
		/// </summary>
		public void Dispose()
		{
			DestroyImmediate(this.gameObject);
		}

		/// <summary>
		/// Returns the current Unity frame number.
		/// </summary>
		/// <value>The current Unity frame number.</value>
		int ITestScheduler.FrameNow => Time.frameCount;

		/// <summary>
		/// Returns the system time using <see cref="DateTimeOffset"/>.<see cref="DateTimeOffset.Now"/>.
		/// </summary>
		/// <value><see cref="DateTimeOffset"/>.<see cref="DateTimeOffset.Now"/></value>
		DateTimeOffset ITestScheduler.TimeNow => DateTimeOffset.Now;

		/// <inheritdoc cref="ITestScheduler.RegisterPollCallback"/>
		IDisposable ITestScheduler.RegisterPollCallback(Action action) =>
			this.poller.RegisterPollCallback(action);

		private void Update() => this.poller.Poll();
	}
}
