using Responsible.Editor;
using UnityEditor;

namespace Responsible.EditorSetup
{
	public static class HotKeys
	{
		[MenuItem("Hot Keys/Show State %&S")]
		public static void ShowStateWindow() => TestOperationStatusWindow.ShowWindow();
	}
}
