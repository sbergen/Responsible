using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Responsible.Utilities;

namespace Responsible.State
{
	public class StateStringBuilder : IndentedStringBuilder<StateStringBuilder>
	{
		public static string MakeState(IOperationState state)
		{
			var builder = new StateStringBuilder();
			state.BuildDescription(builder);
			return builder.ToString();
		}

		public void AddInstruction(
			IOperationState operation,
			string description) =>
			this.AddStatus(operation, description);

		public void AddWait(
			string description,
			IOperationState operation,
			[CanBeNull] Action<StateStringBuilder> extraContext = null)
			=> this.AddStatus(operation, description, extraContext);

		public void AddContinuation(
			IOperationState first,
			[CanBeNull] IOperationState second)
		{
			first.BuildDescription(this);
			second?.BuildDescription(this);
		}

		public void AddResponder(
			string description,
			IOperationState responder,
			IOperationState wait,
			[CanBeNull] IOperationState instruction)
		{
			var statusLine = responder.Status.MakeStatusLine(description);
			if (responder.Status is OperationStatus.Completed)
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

		public void AddUntilResponder(
			string respondToDescription,
			IOperationState<IOperationState> responder,
			string untilDescription,
			IOperationState condition)
			=> this
				.AddOptional(untilDescription, condition)
				.AddOptional(respondToDescription, responder);

		public void AddExpectWithin(
			TimeSpan timeout,
			IOperationState operation)
			=> this.AddIndented(
				operation.Status.MakeStatusLine($"EXPECT WITHIN {timeout:g}"),
				operation.BuildDescription);

		public void AddParentWithChildren(
			string parentDescription,
			IOperationState parentState,
			IEnumerable<IOperationState> children)
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
			[CanBeNull] IOperationState child)
			=> child != null
				? this.AddIndented(description, child.BuildDescription)
				: this.Add($"{description} ...");

		private StateStringBuilder AddStatus(
			IOperationState state,
			string description,
			[CanBeNull] Action<StateStringBuilder> extraContext = null) => this.AddIndented(
			state.Status.MakeStatusLine(description),
			_ =>
			{
				if (state.Status is OperationStatus.Failed failed)
				{
					this.AddEmptyLine();

					var e = failed.Error;
					this.AddIndented(
						"Failed with:",
						b => b.Add($"{e.GetType()}: '{TruncatedExceptionMessage(e)}'"));

					this.AddEmptyLine();

					this.AddIndented("Test Instruction stack:", b =>
					{
						foreach (var sourceLine in failed.SourceContext.SourceLines)
						{
							b.Add(sourceLine);
						}
					});

					this.AddEmptyLine();
				}

				if (state.Status is OperationStatus.Failed || state.Status is OperationStatus.Waiting)
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