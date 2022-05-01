using System.Diagnostics.CodeAnalysis;
using Responsible.Editor.Utilities;
using UnityEditor;

namespace Responsible.Editor
{
	// No easy way to test this in CI, and the core logic is already covered => Exclude
	[ExcludeFromCodeCoverage]
	public class TestOperationStatusWindow : EditorWindow
	{
		private TestOperationStatusWindowState state;

		[MenuItem("Window/Responsible/Operation State")]
		public static void ShowWindow()
		{
			var window = GetWindow<TestOperationStatusWindow>(utility: false, "Test Operation Status");
			window.Show();
		}

		private void OnEnable()
		{
			var style = this.rootVisualElement.style;
			style.paddingTop = style.paddingLeft = style.paddingRight = 5;
			this.state = new TestOperationStatusWindowState(
				this.rootVisualElement,
				TestInstructionExecutor.SubscribeToStates,
				PlayModeStateChangeListener.RegisterCallback);
		}

		private void OnDestroy() => this.state.Dispose();

		private void Update() => this.state.Update();
	}
}
