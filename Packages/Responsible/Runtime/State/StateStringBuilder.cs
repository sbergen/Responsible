using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.Utilities;
using UniRx;

namespace Responsible.State
{
	public class StateStringBuilder : IndentedStringBuilder<StateStringBuilder>
	{
		public static string MakeState(IOperationState state)
		{
			var builder = new StateStringBuilder();
			state.BuildFailureContext(builder);
			return builder.ToString();
		}

		public void AddInstruction(
			IOperationState operation,
			string description,
			SourceContext sourceContext) => this
			.AddIndented(
				operation.Status.MakeStatusLine(description),
				b =>
				{
					b.Add(sourceContext.ToString());
					if (operation.Status is OperationStatus.Failed failed)
					{
						b.Add(FailureMessage(failed.Error));
					}

					// TODO other details
				});

		public void AddWait(
			string description,
			IOperationState operation,
			[CanBeNull] Action<StateStringBuilder> extraContext = null)
			=> this.AddIndented(operation.Status.MakeStatusLine(description), extraContext);

		public void AddContinuation(
			IOperationState first,
			[CanBeNull] IOperationState second)
		{
			first.BuildFailureContext(this);
			second?.BuildFailureContext(this);
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
				.AddOptional(respondToDescription, responder)
				.AddOptional(untilDescription, condition);

		public void AddExpectWithin(
			TimeSpan timeout,
			IOperationState operation,
			SourceContext sourceContext)
			=> this.AddIndented(
				$"EXPECT WITHIN {timeout:g}",
				b =>
				{
					b.Add(sourceContext.ToString());
					operation.BuildFailureContext(b);
				});

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
						child.BuildFailureContext(b);
					}
				});

		private StateStringBuilder AddOptional(
			string description,
			[CanBeNull] IOperationState child)
			=> child != null
				? this.AddIndented(description, child.BuildFailureContext)
				: this.Add($"{description} ...");

		private static string FailureMessage(Exception e)
			=> $"FAILED WITH: {e.GetType().Name}: '{TruncatedExceptionMessage(e)}'";

		private static string TruncatedExceptionMessage(Exception e) => new string(e.Message
			.Select(Indexed.Make)
			.TakeWhile(indexed => indexed.Index < 50 && indexed.Value != '\n')
			.Select(indexed => indexed.Value)
			.ToArray());
	}
}