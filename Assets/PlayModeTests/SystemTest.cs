using System.Collections;
using NUnit.Framework;
using Responsible;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Responsible.Responsibly;
using Object = UnityEngine.Object;

namespace PlayModeTests
{
	[TestFixture]
	public abstract class SystemTest
	{
		private MockInput mockInput;

		protected TestInstructionExecutor TestInstructionExecutor { get; private set; }

		[UnitySetUp]
		public IEnumerator SetUp()
		{
			this.TestInstructionExecutor = new TestInstructionExecutor();
			this.mockInput = new MockInput();
			PlayerInput.Instance = this.mockInput;

			yield return SceneManager.LoadSceneAsync("Scenes/Example");
		}

		[TearDown]
		public void TearDown()
		{
			this.TestInstructionExecutor.Dispose();
			PlayerInput.Instance = null;
		}

		// These operations are written in a way that allows the components to
		// be created later: They do not care if they exist right now or not.
		// We could e.g. make the target area appear and disappear in a future version,
		// and these utilities could easily be modified to support this.

		protected ITestResponder<Unit> TriggerHit(bool shouldHit) => WaitForMainComponents()
			.AndThen(components => WaitForCondition(
				$"Player object is within target area: {shouldHit}",
				() => PlayerIsOnTarget(components) == shouldHit,
				() => components))
			.ThenRespondWith("Trigger hit for this frame", _ => this.mockInput.Trigger());

		protected static Marker[] GetAllMarkers() => Object.FindObjectsOfType<Marker>();

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
	}
}
