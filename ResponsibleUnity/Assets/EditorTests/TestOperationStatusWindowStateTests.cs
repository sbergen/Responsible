using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using Responsible.Editor;
using Responsible.State;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Responsible.EditorTests
{
	public class TestOperationStatusWindowStateTests
	{
		private TestInstructionExecutor.StateNotificationCallback notificationsCallback;
		private Action<PlayModeStateChange> playModeCallback;
		private TestOperationStatusWindowState state;
		private ScrollView scrollView;

		// Resolves the elements into a representation of what things look like
		private class VisualState
		{
			public readonly List<string> CurrentOperations = new List<string>();
			public readonly string PreviousOperation;

			public void AssertEmpty()
			{
				Assert.IsEmpty(this.CurrentOperations);
				Assert.IsNull(this.PreviousOperation);
			}

			public VisualState(VisualElement parent)
			{
				var foundCurrentTitle = false;
				var foundPreviousTitle = false;
				foreach (var label in parent.Query<Label>().Build().ToList()
					.Where(label => !string.IsNullOrEmpty(label.text)))
				{
					if (label.text == "Currently executing operations:")
					{
						Assert.IsFalse(foundCurrentTitle, "Should have only one title for current operations");
						foundCurrentTitle = true;
					}
					else if (label.text == "Last finished operation:")
					{
						Assert.IsFalse(foundPreviousTitle, "Should have only one title for previous operations");
						Assert.IsTrue(
							foundCurrentTitle, "Current operations title should precede previous operation title");
						foundPreviousTitle = true;
					}
					else if (foundPreviousTitle)
					{
						Assert.IsNull(this.PreviousOperation, "Should have only one previous operation");
						this.PreviousOperation = label.text;
					}
					else if (foundCurrentTitle)
					{
						this.CurrentOperations.Add(label.text);
					}
					else
					{
						Assert.Fail($"Found label before any titles: {label.text}");
					}
				}

				Assert.IsTrue(foundCurrentTitle, "Should have title for current instructions");
				Assert.IsTrue(foundPreviousTitle, "Should have title for previous instruction");
			}
		}

		private static IDisposable MakeCallbackDisposable(Action callback)
		{
			var disposable = Substitute.For<IDisposable>();
			disposable.When(_ => _.Dispose()).Do(_ => callback());
			return disposable;
		}

		[SetUp]
		public void SetUp()
		{
			var root = new VisualElement();
			this.state = new TestOperationStatusWindowState(
				root,
				callback =>
				{
					this.notificationsCallback = callback;
					return MakeCallbackDisposable(() => this.notificationsCallback = null);
				},
				callback =>
				{
					this.playModeCallback = callback;
					return MakeCallbackDisposable(() => this.playModeCallback = null);
				});
			this.scrollView = root.Q<ScrollView>();

			this.playModeCallback(PlayModeStateChange.EnteredPlayMode);

			// Precondition
			Assert.NotNull(this.scrollView);
		}

		[TearDown]
		public void TearDown()
		{
			this.state.Dispose();
		}

		[Test]
		public void Construction_AddsOnlyTitleLabels()
		{
			new VisualState(this.scrollView).AssertEmpty();
		}

		[Test]
		public void StartedNotification_AddsStateLabel()
		{
			var operationState1 = new FakeOperationState("Fake State 1");
			this.notificationsCallback(TestOperationStateTransition.Started, operationState1);
			var operationState2 = new FakeOperationState("Fake State 2");
			this.notificationsCallback(TestOperationStateTransition.Started, operationState2);
			this.state.Update();

			var visualState = new VisualState(this.scrollView);
			CollectionAssert.AreEqual(
				new[] { operationState1.StringRepresentation, operationState2.StringRepresentation },
				visualState.CurrentOperations);
			Assert.IsNull(visualState.PreviousOperation);
		}

		[Test]
		public void FinishedNotification_MovesStateLabelToPrevious()
		{
			var operationState = new FakeOperationState("Fake State");
			this.notificationsCallback(TestOperationStateTransition.Started, operationState);
			this.state.Update();
			this.notificationsCallback(TestOperationStateTransition.Finished, operationState);

			var visualState = new VisualState(this.scrollView);
			Assert.IsEmpty(visualState.CurrentOperations);
			Assert.AreEqual(operationState.StringRepresentation, visualState.PreviousOperation);
		}

		[Test]
		[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed", Justification = "Mock checks")]
		public void StateUpdates_Stop_WhenExitingPlayMode()
		{
			var stateMock = Substitute.For<ITestOperationState>();
			this.notificationsCallback(TestOperationStateTransition.Started, stateMock);

			// sanity check
			this.state.Update();
			stateMock.Received(1).ToString();
			// end sanity check

			stateMock.ClearReceivedCalls();
			this.playModeCallback(PlayModeStateChange.ExitingPlayMode);
			this.state.Update();
			stateMock.DidNotReceive().ToString();
		}

		[Test]
		public void CurrentOperations_AreCleared_WhenEnteringPlayMode()
		{
			var operationState = new FakeOperationState("Fake State");
			this.notificationsCallback(TestOperationStateTransition.Started, operationState);
			this.state.Update();
			this.playModeCallback(PlayModeStateChange.ExitingPlayMode);
			this.playModeCallback(PlayModeStateChange.EnteredPlayMode);

			new VisualState(this.scrollView).AssertEmpty();
		}

		[Test]
		public void FinishedNotification_OnlyLogsWarning_WhenNotFound()
		{
			var operationState = new FakeOperationState("Fake State");
			this.notificationsCallback(TestOperationStateTransition.Finished, operationState);

			LogAssert.Expect(LogType.Warning, new Regex("Could not find"));
			new VisualState(this.scrollView).AssertEmpty();
		}

		[Test]
		public void Dispose_StopsListeningToNotifications()
		{
			this.state.Dispose();
			Assert.IsNull(this.notificationsCallback);
			Assert.IsNull(this.playModeCallback);
		}

		[Test]
		public void MultipleOperations_OperateCorrectly()
		{
			var state1 = new FakeOperationState("Fake State 1");
			var state2 = new FakeOperationState("Fake State 2");
			this.notificationsCallback(TestOperationStateTransition.Started, state1);
			this.state.Update();
			this.notificationsCallback(TestOperationStateTransition.Started, state2);
			this.state.Update();
			this.notificationsCallback(TestOperationStateTransition.Finished, state1);
			this.state.Update();

			var visualState = new VisualState(this.scrollView);
			CollectionAssert.AreEqual(
				new[] { state2.StringRepresentation },
				visualState.CurrentOperations);
			Assert.AreEqual(state1.StringRepresentation, visualState.PreviousOperation);
		}

		[Test]
		public void ErrorInState_ContainsErrorInUI()
		{
			this.notificationsCallback(
				TestOperationStateTransition.Started,
				new FakeOperationState(new Exception("Fake Exception")));
			this.state.Update();

			var visualState = new VisualState(this.scrollView);
			Assert.IsTrue(visualState.CurrentOperations.Any(t => t.Contains("Fake Exception")));
		}
	}
}
