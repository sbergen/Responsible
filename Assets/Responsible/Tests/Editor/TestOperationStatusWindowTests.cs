using NUnit.Framework;
using Responsible.Editor;
using UnityEditor;
using UnityEngine;

namespace Responsible.Tests.Editor
{
	public class TestOperationStatusWindowTests
	{
		[Test]
		public void TestOperationStatusWindow_SmokeTest()
		{
			TestOperationStatusWindow.ShowWindow();
			var window = EditorWindow.GetWindow<TestOperationStatusWindow>();
			window.Update();
			window.Close();
		}
	}
}
