using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Responsible.Utilities;

namespace Responsible.State
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
			ContinuationState continuation)
		{
			first.BuildDescription(this);

			switch (continuation)
			{
				case ContinuationState.Available available:
					available.State.BuildDescription(this);
					break;

				case ContinuationState.NotAvailable notAvailable:
					this.AddStatus(notAvailable.CreationStatus, "...");
					break;
			}
		}

		internal void AddUntilResponder<T>(
			string respondToDescription,
			ITestOperationState<IMultipleTaskSource<ITestOperationState<T>>> responder,
			string untilDescription,
			ITestOperationState condition)
			=> this
				.AddOptional(untilDescription, condition)
				.AddOptional(respondToDescription, responder);

		internal void AddResponder(IBasicResponderState responder) => this.AddResponder(
			responder.Description,
			primaryState: responder.InstructionState ?? responder,
			responder.WaitState,
			responder.InstructionState);

		internal void AddExpectWithin(
			ITestOperationState expectOperation,
			TimeSpan timeout,
			ITestOperationState operation)
		{
			var timeoutString = $"{timeout.TotalSeconds:0.00 s}";

			if (operation is IBoxedOperationState boxedState)
			{
				operation = boxedState.WrappedState;
			}

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
					$"{responderState.Description} CONDITION EXPECTED WITHIN {timeoutString}",
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

		private StateStringBuilder AddOptional(
			string description,
			[CanBeNull] ITestOperationState child)
			=> child != null
				? this.AddIndented(description, child.BuildDescription)
				: this.Add($"{description} ...");

		private StateStringBuilder AddStatus(
			TestOperationStatus status,
			string description) => this
			.AddIndented(
				status.MakeStatusLine(description),
				_ => this.AddFailureDetails(status));

		private StateStringBuilder AddStatus(
			TestOperationStatus status,
			string description,
			[CanBeNull] Action<StateStringBuilder> extraContext)
		{
			this.AddStatus(status, description);

			if (status is TestOperationStatus.Failed || status is TestOperationStatus.Waiting)
			{
				this.AddEmptyLine();

				if (extraContext != null)
				{
					extraContext(this);
				}
				else
				{
					this.AddNestedDetails(
						"No extra context provided",
						b => b.AddDetails(
							"Consider using the 'extraContext' parameter to get more descriptive output!"));
				}
			}

			return this;
		}

		private StateStringBuilder AddFailureDetails(
			TestOperationStatus status)
		{
			if (status is TestOperationStatus.Failed failed)
			{
				this.AddEmptyLine();

				var e = failed.Error;
				this.AddIndented(
					"Failed with:",
					b => b.AddNestedDetails(
						$"{e.GetType()}:",
						nested => nested.AddDetails(e.Message)));

				this.AddEmptyLine();

				this.AddIndented("Test operation stack:", b =>
				{
					foreach (var sourceLine in failed.SourceContext.SourceLines)
					{
						b.Add(sourceLine);
					}
				});
			}

			return this;
		}
	}
}
