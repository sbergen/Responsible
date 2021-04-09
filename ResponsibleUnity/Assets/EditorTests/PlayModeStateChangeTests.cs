using NUnit.Framework;
using Responsible.Editor.Utilities;

namespace Responsible.EditorTests
{
	public class PlayModeStateChangeTests
	{
		[Test]
		public void SmokeTest()
		{
			// Can't really test that this works with Unity, so just do a smoke test...
			Assert.DoesNotThrow(() =>
				PlayModeStateChangeListener.RegisterCallback(_ => { }).Dispose());
		}
	}
}
