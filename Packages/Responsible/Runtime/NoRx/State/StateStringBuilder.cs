using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Responsible.Utilities;

namespace Responsible.NoRx.State
{
	/// <summary>
	/// Utility class for building string representations of test operation execution states.
	/// </summary>
	public class StateStringBuilder : IndentedStringBuilder<StateStringBuilder>
	{
		/// <summary>
		/// Adds details to the state of an operation.
		/// </summary>
		/// <param name="details">Details to add, may be multiple lines</param>
		/// <remarks>
		/// The input will be split by the newline character,
		/// and added as separate lines, to respect indentation.
		/// </remarks>
		public void AddDetails(string details)
		{
			// Respect indentation by splitting to lines
			foreach (var line in details.Split('\n'))
			{
				this.Add(line);
			}
		}

		/// <summary>
		/// Adds nested details using indentation.
		/// This method may be called recursively from <paramref name="addNested"/>
		/// to add multiple levels of indentation.
		/// </summary>
		/// <param name="description">Description of the details.</param>
		/// <param name="addNested">Action to add more details at an increased indentation level.</param>
		public void AddNestedDetails(string description, Action<StateStringBuilder> addNested)
		{
			this.AddIndented(description, addNested);
		}

		internal static string MakeState(ITestOperationState state)
		{
			var builder = new StateStringBuilder();
			state.BuildDescription(builder);
			return builder.ToString();
		}

		internal void AddInstruction(
			ITestOperationState operation,
			string description) =>
			this.AddStatus(operation.Status, description);

		internal void AddSelect<T1, T2>(
			ITestOperationState primaryState,
			TestOperationStatus selectStatus)
		{
			primaryState.BuildDescription(this);
			this.AddStatus(
				selectStatus,
				$"SELECT {typeof(T1).Name} -> {typeof(T2).Name}");
		}

		internal void AddWait(IDiscreteWaitConditionState operation)
			=> this.AddStatus(operation.Status, operation.Description, operation.ExtraContext);

		internal void AddContinuation(
			ITestOperationState first,
			[CanBeNull] ITestOperationState second)
		{
			first.BuildDescription(this);
			if (second != null)
			{
				second.BuildDescription(this);
			}
			else
			{
				this.AddStatus(TestOperationStatus.NotExecuted.Instance, "...");
			}
		}

		internal void AddUntilResponder(
			string respondToDescription,
			ITestOperationState<ITestOperationState> responder,
			string untilDescription,
			ITestOperationState condition)
			=> this
				.AddOptional(untilDescription, condition)
				.AddOptional(respondToDescription, responder);

		internal void AddResponder(IBasicResponderState responder) => this.AddResponder(
			responder.Description,
			responder.InstructionState ?? responder,
			responder.WaitState,
			responder.InstructionState);

		internal void AddExpectWithin(
			ITestOperationState expectOperation,
			TimeSpan timeout,
			ITestOperationState operation)
		{
			var timeoutString = timeout > TimeSpan.FromMinutes(1)
				? $"{timeout:g}"
				: $"{timeout.TotalSeconds:0.00 s}";

			if (operation is IDiscreteWaitConditionState discreteSate)
			{
				this.AddStatus(
					expectOperation.Status,
					$"{discreteSate.Description} EXPECTED WITHIN {timeoutString}",
					discreteSate.ExtraContext);
			}
			else if (operation is IBasicResponderState responderState)
			{
				this.AddResponder(
					$"{responderState.Description} EXPECTED WITHIN {timeoutString}",
					expectOperation,
					responderState.WaitState,
					responderState.InstructionState);
			}
			else
			{
				this.AddParentWithChildren(
					$"EXPECT WITHIN {timeoutString}",
					expectOperation,
					new[] { operation });
			}
		}

		internal StateStringBuilder AddParentWithChildren(
			string parentDescription,
			ITestOperationState parentState,
			IEnumerable<ITestOperationState> children) => this
			.AddIndented(
				parentState.Status.MakeStatusLine(parentDescription),
				b =>
				{
					foreach (var child in children)
					{
						child.BuildDescription(b);
					}
				})
			.AddFailureDetails(parentState.Status);

		internal void AddToPreviousLineWithChildren(
			string addToPrevious,
			IEnumerable<ITestOperationState> children)
		{
			this.AddToPreviousLine(addToPrevious);
			foreach (var child in children)
			{
				child.BuildDescription(this);
			}
		}

		private void AddResponder(
			string description,
			ITestOperationState primaryState,
			ITestOperationState wait,
			[CanBeNull] ITestOperationState instruction)
		{
			var status = primaryState.Status;
			var statusLine = status.MakeStatusLine(description);
			if (status is TestOperationStatus.Completed ||
				status is TestOperationStatus.NotExecuted)
			{
				this.Add(statusLine);
			}
			else
			{
				this.AddIndented(statusLine, b => b
					.AddOptional("WAIT FOR", wait)
					.AddOptional("THEN RESPOND WITH", instruction));

				// Add primary state failure details, if we didn't have failure details yet
				var failureIncluded =
					wait.Status is TestOperationStatus.Failed ||
					instruction?.Status is TestOperationStatus.Failed;
				if (!failureIncluded)
				{
					this.AddFailureDetails(status);
				}
			}
		}

		private StateStringBuilder AddOptional(
			string description,
			[CanBeNull] ITestOperationState child)
			=> child != null
				? this.AddIndented(description, child.BuildDescription)
				: this.Add($"{description} ...");

		private StateStringBuilder AddStatus(
			TestOperationStatus status,
			string description,
			[CanBeNull] Action<StateStringBuilder> extraContext = null) => this
			.AddIndented(
				status.MakeStatusLine(description),
				_ => this.AddFailureDetails(status, extraContext));

		private StateStringBuilder AddFailureDetails(
			TestOperationStatus status,
			[CanBeNull] Action<StateStringBuilder> extraContext = null)
		{
			if (status is TestOperationStatus.Failed failed)
			{
				this.AddEmptyLine();

				var e = failed.Error;
				this.AddIndented(
					"Failed with:",
					b => b.Add($"{e.GetType()}: '{TruncatedExceptionMessage(e)}'"));

				this.AddEmptyLine();

				this.AddIndented("Test operation stack:", b =>
				{
					foreach (var sourceLine in failed.SourceContext.SourceLines)
					{
						b.Add(sourceLine);
					}
				});

				this.AddEmptyLine();
			}

			if (status is TestOperationStatus.Failed || status is TestOperationStatus.Waiting)
			{
				extraContext?.Invoke(this);
			}

			return this;
		}

		private static string TruncatedExceptionMessage(Exception e) => new string(e.Message
			.Select(Indexed.Make)
			.TakeWhile(indexed => indexed.Index < 100 && indexed.Value != '\n')
			.Select(indexed => indexed.Value)
			.ToArray());
	}
}
