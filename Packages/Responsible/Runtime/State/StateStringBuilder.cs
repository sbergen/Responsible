using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Responsible.Utilities;

namespace Responsible.State
{
	public class StateStringBuilder : IndentedStringBuilder<StateStringBuilder>
	{
		public void AddDetails(string details)
		{
			// Respect indentation by splitting to lines
			foreach (var line in details.Split('\n'))
			{
				this.Add(line);
			}
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
			this.AddStatus(operation, description);

		internal void AddSelect<T1, T2>(
			ITestOperationState primaryState,
			ITestOperationState selectState)
		{
			primaryState.BuildDescription(this);
			this.AddStatus(
				selectState,
				$"SELECT {typeof(T1).Name} -> {typeof(T2).Name}");
		}

		internal void AddWait(
			string description,
			ITestOperationState operation,
			[CanBeNull] Action<StateStringBuilder> extraContext = null)
			=> this.AddStatus(operation, description, extraContext);

		internal void AddContinuation(
			ITestOperationState first,
			[CanBeNull] ITestOperationState second)
		{
			first.BuildDescription(this);
			second?.BuildDescription(this);
		}

		internal void AddResponder(
			string description,
			ITestOperationState responder,
			ITestOperationState wait,
			[CanBeNull] ITestOperationState instruction)
		{
			// Needs special handling, as the instruction execution is not part of the responder stream
			var operationForStatus = instruction ?? responder;
			var statusLine = operationForStatus.Status.MakeStatusLine(description);
			if (operationForStatus.Status is TestOperationStatus.Completed ||
				operationForStatus.Status is TestOperationStatus.NotExecuted)
			{
				this.Add(statusLine);
			}
			else
			{
				this.AddIndented(statusLine, b => b
					.AddOptional("WAIT FOR", wait)
					.AddOptional("THEN RESPOND WITH", instruction));
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

		internal void AddExpectWithin(
			ITestOperationState expectOperation,
			TimeSpan timeout,
			ITestOperationState operation)
			=> this.AddIndented(
				expectOperation.Status.MakeStatusLine($"EXPECT WITHIN {timeout:g}"),
				operation.BuildDescription);

		internal void AddParentWithChildren(
			string parentDescription,
			ITestOperationState parentState,
			IEnumerable<ITestOperationState> children)
			=> this.AddIndented(
				parentState.Status.MakeStatusLine(parentDescription),
				b =>
				{
					foreach (var child in children)
					{
						child.BuildDescription(b);
					}
				});

		private StateStringBuilder AddOptional(
			string description,
			[CanBeNull] ITestOperationState child)
			=> child != null
				? this.AddIndented(description, child.BuildDescription)
				: this.Add($"{description} ...");

		private StateStringBuilder AddStatus(
			ITestOperationState state,
			string description,
			[CanBeNull] Action<StateStringBuilder> extraContext = null) => this.AddIndented(
			state.Status.MakeStatusLine(description),
			_ =>
			{
				if (state.Status is TestOperationStatus.Failed failed)
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

				if (state.Status is TestOperationStatus.Failed || state.Status is TestOperationStatus.Waiting)
				{
					extraContext?.Invoke(this);
				}
			});

		private static string TruncatedExceptionMessage(Exception e) => new string(e.Message
			.Select(Indexed.Make)
			.TakeWhile(indexed => indexed.Index < 100 && indexed.Value != '\n')
			.Select(indexed => indexed.Value)
			.ToArray());
	}
}