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
		public static string MakeState<T>(IOperationState<T> state)
		{
			var builder = new StateStringBuilder();
			state.BuildFailureContext(builder);
			return builder.ToString();
		}

		public void AddInstruction<T>(
			IOperationState<T> operation,
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

		public void AddWait<T>(
			string description,
			IOperationState<T> operation,
			[CanBeNull] Action<StateStringBuilder> extraContext = null)
			=> this.AddIndented(operation.Status.MakeStatusLine(description), extraContext);

		public void AddContinuation<T1, T2>(
			IOperationState<T1> first,
			[CanBeNull] IOperationState<T2> second)
			=> this
				.AddIndented("FIRST", first.BuildFailureContext)
				.AddOptional("AND THEN", second);

		public void AddResponder<T1, T2>(
			string description,
			IOperationState<T1> wait,
			[CanBeNull] IOperationState<T2> instruction)
			=> this
				.AddIndented(description, b => b
					.AddOptional("WAIT FOR", wait)
					.AddOptional("THEN RESPOND WITH", instruction));

		public void AddOptionalResponder<T>(
			string respondToDescription,
			IOperationState<IOperationState<Unit>> responder,
			string untilDescription,
			IOperationState<T> condition)
			=> this
				.AddOptional(respondToDescription, responder)
				.AddOptional(untilDescription, condition);

		public void AddExpectWithin<T>(
			TimeSpan timeout,
			IOperationState<T> operation,
			SourceContext sourceContext)
			=> this.AddIndented(
				$"EXPECT WITHIN {timeout:g}",
				b =>
				{
					b.Add(sourceContext.ToString());
					operation.BuildFailureContext(b);
				});

		public void AddParentWithChildren<T1, T2>(
			string parentDescription,
			IOperationState<T1> parentState,
			IEnumerable<IOperationState<T2>> children)
			=> this.AddIndented(
				parentState.Status.MakeStatusLine(parentDescription),
				b =>
				{
					foreach (var child in children)
					{
						child.BuildFailureContext(b);
					}
				});

		private StateStringBuilder AddOptional<T>(
			string description,
			[CanBeNull] IOperationState<T> child)
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