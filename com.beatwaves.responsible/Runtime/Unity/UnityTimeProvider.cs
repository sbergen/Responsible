using System;
using Responsible.Utilities;
using UnityEngine;

namespace Responsible.Unity
{
	public class UnityTimeProvider : MonoBehaviour, ITimeProvider
	{
		private readonly RetryingPoller poller = new RetryingPoller();

		public int FrameNow => Time.frameCount;
		public DateTimeOffset TimeNow => DateTimeOffset.Now;

		public IDisposable RegisterPollCallback(Action action) =>
			this.poller.RegisterPollCallback(action);

		private void Update() => this.poller.Poll();
	}
}
