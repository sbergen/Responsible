using System;
using System.Collections.Generic;
using System.Linq;
using Responsible.State;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Responsible.Editor
{
	public class TestOperationStatusWindow : EditorWindow
	{
		private readonly List<ITestOperationState> activeStates = new List<ITestOperationState>();

		private IDisposable subscription;
		private Vector2 scrollPosition;

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
				this.subscription = this.SubscribeToStates();
			}

			if (this.activeStates.Count == 0)
			{
				GUILayout.Label("No operations executing");
			}
			else
			{
				this.DrawStates();
			}
		}

		private void Update()
		{
			if (this.activeStates.Count > 0)
			{
				this.Repaint();
			}
		}

		private void OnDestroy()
		{
			this.subscription?.Dispose();
		}

		private IDisposable SubscribeToStates() => TestInstructionExecutor.StateNotifications.Subscribe(notification =>
		{
			switch (notification)
			{
				case TestOperationStateNotification.Finished finished:
					this.activeStates.Remove(finished.State);
					break;
				case TestOperationStateNotification.Started started:
					this.activeStates.Add(started.State);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(notification));
			}

			this.Repaint();
		});

		private void DrawStates()
		{
			this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition);

			try
			{
				foreach (var state in this.activeStates)
				{
					GUILayout.Label(state.ToString());
					GUILayout.Label("----------");
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Exception getting test operation state:\n{e}");
			}
			finally
			{
				EditorGUILayout.EndScrollView();
			}
		}
	}
}
