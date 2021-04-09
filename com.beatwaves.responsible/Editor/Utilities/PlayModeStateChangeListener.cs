using System;
using UnityEditor;

namespace Responsible.Editor.Utilities
{
	internal static class PlayModeStateChangeListener
	{
		public static IDisposable RegisterCallback(Action<PlayModeStateChange> callback) =>
			new EventListener(callback);

		private class EventListener : IDisposable
		{
			private readonly Action<PlayModeStateChange> handler;

			public EventListener(Action<PlayModeStateChange> handler)
			{
				this.handler = handler;
				EditorApplication.playModeStateChanged += handler;
			}

			public void Dispose()
			{
				EditorApplication.playModeStateChanged -= this.handler;
			}
		}
	}
}
