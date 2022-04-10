using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Responsible;
using Responsible.Bdd;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Responsible.Responsibly;
using Object = UnityEngine.Object;

namespace ResponsibleGame.PlayModeTests
{
	[TestFixture]
	public abstract class SystemTest : BddTest
	{
		private string scenePath;
		private MockInput mockInput;

		[OneTimeSetUp]
		public void ResolveScenePath()
		{
			// This is a bit complicated, as we don't want to force you to add the sample
			// scene into your build settings, and I don't want to hard-code the samples location.
			// Note that will only work in the editor, but you could just use LoadSceneAsync normally in player tests.
			var sampleDirectory = new FileInfo(GetCallerPath()).Directory?.Parent?.FullName;
			var assetsDirectory = new DirectoryInfo(Application.dataPath).FullName;
			this.scenePath = $"Assets{sampleDirectory?.Substring(assetsDirectory.Length)}/Scenes/ResponsibleGame.unity";
		}

		[UnitySetUp]
		public IEnumerator SetUp()
		{
			this.mockInput = new MockInput();
			PlayerInput.Instance = this.mockInput;

			yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
				this.scenePath,
				new LoadSceneParameters(LoadSceneMode.Additive));
		}

		[TearDown]
		public void TearDown()
		{
			PlayerInput.Instance = null;
		}

		[UnityTearDown]
		public IEnumerator AsyncTearDown()
		{
			yield return SceneManager.UnloadSceneAsync(this.scenePath);
		}

		// These operations are written in a way that allows the components to
		// be created later: They do not care if they exist right now or not.
		// We could e.g. make the target area appear and disappear in a future version,
		// and these utilities could easily be modified to support this.

		protected ITestResponder<object> TriggerHit(bool shouldHit) => WaitForMainComponents()
			.AndThen(components => WaitForCondition(
				$"Player object is within target area: {shouldHit}",
				() => PlayerIsOnTarget(components) == shouldHit))
			.ThenRespondWith($"Trigger {(shouldHit ? "hit" : "miss")}", this.MockTriggerInput());

		protected ITestInstruction<object> MockTriggerInput() =>
			Do("Mock trigger input", () => this.mockInput.Trigger());

		protected static Marker[] GetAllMarkers() => Object.FindObjectsOfType<Marker>();

		protected static Status ExpectStatusInstance()
		{
			// Sometimes you want to just expect things to be there.
			// Not everything needs to be async :)
			var status = Object.FindObjectOfType<Status>();
			Assert.IsNotNull(status, "Status is expected to not be null");
			return status;
		}

		private static ITestWaitCondition<MainComponents> WaitForMainComponents()
			=> WaitForConditionOn(
				"Wait for main components",
				() => new MainComponents(
					Object.FindObjectOfType<PlayerObject>(),
					Object.FindObjectOfType<TargetArea>()),
				components => components.BothSet);

		private static bool PlayerIsOnTarget(MainComponents components)
		{
			// Whether or not this duplication is good or not is a matter of testing philosophy....
			var position = ((RectTransform)components.PlayerObject.transform).localPosition;
			return ((RectTransform)components.TargetArea.transform).rect.Contains(position);
		}

		private static string GetCallerPath([CallerFilePath] string callerPath = null) => callerPath;
	}
}
