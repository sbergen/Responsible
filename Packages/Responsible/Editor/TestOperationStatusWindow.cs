using System;
using Responsible;
using Responsible.State;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Responsible.Editor
{
	public class TestOperationStatusWindow : EditorWindow
	{
		private IDisposable subscription;
		private ITestOperationState currentState;

		[MenuItem("Window/Responsible/Operation State")]
		public static void ShowWindow()
		{
			var window = GetWindow<TestOperationStatusWindow>(utility: false, "Test Operation State");
			window.Show();
		}

		private void OnGUI()
		{
			if (this.subscription == null)
			{
				this.subscription = TestInstructionExecutor.StateNotifications
					.Subscribe(notification =>
					{
						switch (notification)
						{
							case TestOperationStateNotification.Finished _:
								this.currentState = null;
								break;
							case TestOperationStateNotification.Started started:
								this.currentState = started.State;
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(notification));
						}

						this.Repaint();
					});
			}

			GUILayout.Label(this.currentState?.ToString() ?? "No operations executing");
		}

		private void Update()
		{
			if (this.currentState != null)
			{
				this.Repaint();
			}
		}

		private void OnDestroy()
		{
			this.subscription?.Dispose();
		}
	}
}
