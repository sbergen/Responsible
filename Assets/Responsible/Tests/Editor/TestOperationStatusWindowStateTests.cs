using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Responsible.Editor;
using Responsible.State;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Responsible.Tests.Editor
{
	public class TestOperationStatusWindowStateTests
	{
		private class FakeOperationState : ITestOperationState
		{
			public readonly string StringRepresentation;
			public readonly Exception Exception;

			public FakeOperationState(string stringRepresentation)
			{
				this.StringRepresentation = stringRepresentation;
			}

			public FakeOperationState(Exception exception)
			{
				this.Exception = exception;
			}

			public TestOperationStatus Status => throw new System.NotImplementedException();
			public void BuildDescription(StateStringBuilder builder) => throw new System.NotImplementedException();
			public override string ToString() => this.Exception != null
				? throw this.Exception
				: this.StringRepresentation;
		}

		private Subject<TestOperationStateNotification> notifications;
		private TestOperationStatusWindowState state;
		private ScrollView scrollView;

		[SetUp]
		public void SetUp()
		{
			var root = new VisualElement();
			this.notifications = new Subject<TestOperationStateNotification>();
			this.state = new TestOperationStatusWindowState(root, this.notifications);
			this.scrollView = root.Q<ScrollView>();

			// Precondition
			Assert.NotNull(this.scrollView);
		}

		[TearDown]
		public void TearDown()
		{
			this.state.Dispose();
		}

		[Test]
		public void Construction_AddsNoStateLabel()
		{
			var labels = this.GetAllLabels();
			Assert.AreEqual(1, labels.Count, "Should have a single label");
			Assert.AreEqual(labels[0].text, "No operations executing");
		}

		[Test]
		public void StartedNotification_AddsStateLabel()
		{
			var operationState = new FakeOperationState("Fake State");
			this.notifications.OnNext(new TestOperationStateNotification.Started(operationState));
			this.state.Update();

			Assert.That(this.GetAllLabelTexts(), Contains.Item(operationState.StringRepresentation));
		}

		[Test]
		public void FinishedNotification_RemovesStateLabel()
		{
			var operationState = new FakeOperationState("Fake State");
			this.notifications.OnNext(new TestOperationStateNotification.Started(operationState));
			this.state.Update();
			this.notifications.OnNext(new TestOperationStateNotification.Finished(operationState));

			Assert.That(this.GetAllLabelTexts(), Does.Not.Contain(operationState.StringRepresentation));
		}

		[Test]
		public void FinishedNotification_OnlyLogsWarning_WhenNotFound()
		{
			var operationState = new FakeOperationState("Fake State");
			var preNotificationLabelCount = this.GetAllLabels().Count;
			this.notifications.OnNext(new TestOperationStateNotification.Finished(operationState));

			LogAssert.Expect(LogType.Warning, new Regex("Could not find"));
			Assert.AreEqual(
				preNotificationLabelCount,
				this.GetAllLabels().Count,
				"Should not change label count on mismatched finished notification");
		}

		[Test]
		public void Dispose_StopsListeningToNotifications()
		{
			this.state.Dispose();
			var preNotificationLabelCount = this.GetAllLabels().Count;
			this.notifications.OnNext(new TestOperationStateNotification.Started(default));

			Assert.AreEqual(
				preNotificationLabelCount,
				this.GetAllLabels().Count,
				"Should not add new labels after being disposed");
		}

		[Test]
		public void MultipleOperations_OperateCorrectly()
		{
			var state1 = new FakeOperationState("Fake State 1");
			var state2 = new FakeOperationState("Fake State 2");
			this.notifications.OnNext(new TestOperationStateNotification.Started(state1));
			this.state.Update();
			this.notifications.OnNext(new TestOperationStateNotification.Started(state2));
			this.state.Update();
			this.notifications.OnNext(new TestOperationStateNotification.Finished(state1));
			this.state.Update();

			var texts = this.GetAllLabelTexts();
			Assert.That(texts, Does.Not.Contain(state1.StringRepresentation));
			Assert.That(texts, Contains.Item(state2.StringRepresentation));
		}

		[Test]
		public void ErrorInState_ContainsErrorInUI()
		{
			this.notifications.OnNext(new TestOperationStateNotification.Started(
				new FakeOperationState(new Exception("Fake Exception"))));
			this.state.Update();

			var texts = this.GetAllLabelTexts();
			Assert.IsTrue(texts.Any(t => t.Contains("Fake Exception")));
		}

		private List<Label> GetAllLabels() => this.scrollView.Query<Label>().Build().ToList();
		private List<string> GetAllLabelTexts() => this.GetAllLabels().Select(l => l.text).ToList();
	}
}
