using UnityEditor;

namespace Responsible.Editor
{
	public class TestOperationStatusWindow : EditorWindow
	{
		private TestOperationStatusWindowState state;

		[MenuItem("Window/Responsible/Operation State")]
		public static void ShowWindow()
		{
			var window = GetWindow<TestOperationStatusWindow>(utility: false, "Test Operation State");
			window.Show();
		}

		private void OnEnable()
		{
			this.state = new TestOperationStatusWindowState(
				this.rootVisualElement,
				TestInstructionExecutor.StateNotifications);
		}

		private void OnDestroy() => this.state.Dispose();

		private void Update() => this.state.Update();
	}
}
