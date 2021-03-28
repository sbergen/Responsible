using System;
using System.Collections.Generic;
using Responsible.State;
using UnityEngine;
using UnityEngine.UIElements;

namespace Responsible.Editor
{
	public class TestOperationStatusWindowState : IDisposable
	{
		private readonly List<(ITestOperationState state, Label label)> stateLabels =
			new List<(ITestOperationState state, Label label)>();

		private readonly IDisposable subscription;

		public TestOperationStatusWindowState(
			VisualElement rootElement,
			Func<TestInstructionExecutor.StateNotificationCallback, IDisposable> subscribeFunction)
		{
			var currentOperations = new VisualElement();
			var currentOperationsTitle = new Label("Currently executing operations:");
			currentOperationsTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
			currentOperations.Add(currentOperationsTitle);
			currentOperations.style.paddingBottom = new StyleLength(20);

			var previousOperation = new VisualElement();
			var previousOperationTitle = new Label("Last finished operation:");
			previousOperationTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
			var previousOperationLabel = new Label();
			previousOperation.Add(previousOperationTitle);
			previousOperation.Add(previousOperationLabel);

			var scrollView = new ScrollView();
			rootElement.Add(scrollView);
			scrollView.Add(currentOperations);
			scrollView.Add(previousOperation);

			void AddLabel(ITestOperationState state)
			{
				var label = new Label();
				label.style.paddingBottom = new StyleLength(20);
				this.stateLabels.Add((state, label));
				currentOperations.Add(label);
			}

			void RemoveLabel(ITestOperationState state)
			{
				var i = this.stateLabels.FindIndex(entry => entry.state == state);
				if (i < 0)
				{
					Debug.LogWarning("Could not find label for finished operation!");
				}
				else
				{
					var label = this.stateLabels[i].label;
					previousOperationLabel.text = state.ToString();
					currentOperations.Remove(label);
					this.stateLabels.RemoveAt(i);
				}
			}

			this.subscription = subscribeFunction((type, state) =>
			{
				switch (type)
				{
					case TestOperationStateTransition.Started:
						AddLabel(state);
						break;
					case TestOperationStateTransition.Finished:
						RemoveLabel(state);
						break;
				}
			});
		}

		public void Dispose()
		{
			this.subscription.Dispose();
		}

		public void Update()
		{
			foreach (var (state, label) in this.stateLabels)
			{
				try
				{
					label.text = state.ToString();
				}
				catch (Exception e)
				{
					label.text = $"Error getting state:\n{e}";
				}
			}
		}
	}
}
