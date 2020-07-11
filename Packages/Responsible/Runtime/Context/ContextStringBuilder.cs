using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Responsible.Context
{
	/* TODO
	public class ContextStringBuilder
	{
		private const int IndentChars = 2;
		private const string DescriptionNotAvailable = "No description";
		private const string FailureContextNotAvailable = "No failure context";

		private static readonly Action<ITestOperationContext, ContextStringBuilder> DescriptionBuilder =
			(context, builder) => context.BuildDescription(builder);

		private static readonly Action<ITestOperationContext, ContextStringBuilder> FailureBuilder =
			(context, builder) => context.BuildFailureContext(builder);

		private readonly StringBuilder stringBuilder = new StringBuilder();
		private readonly Action<ITestOperationContext, ContextStringBuilder> contextBuilder;
		private readonly string notAvailableText;

		private int indentAmount;

		internal RunContext RunContext { get; }
		internal WaitContext WaitContext { get; }

		internal static ContextStringBuilder MakeDescription(
			ITestOperationContext rootContext,
			RunContext runContext,
			WaitContext waitContext)
		{
			var builder = new ContextStringBuilder(
				runContext,
				waitContext,
				DescriptionBuilder,
				DescriptionNotAvailable);
			builder.AddRoot(rootContext);
			return builder;
		}

		internal static ContextStringBuilder MakeFailureContext(
			ITestOperationContext rootContext,
			RunContext runContext,
			WaitContext waitContext)
		{
			var builder = new ContextStringBuilder(
				runContext,
				waitContext,
				FailureBuilder,
				FailureContextNotAvailable);
			builder.AddRoot(rootContext);
			return builder;
		}

		private ContextStringBuilder(
			RunContext runContext,
			WaitContext waitContext,
			Action<ITestOperationContext, ContextStringBuilder> contextBuilder,
			string notAvailableText)
		{
			this.RunContext = runContext;
			this.WaitContext = waitContext;
			this.contextBuilder = contextBuilder;
			this.notAvailableText = notAvailableText;
		}

		// Trim out last newline when converting to final string
		public override string ToString() => this.stringBuilder.ToString(0, this.stringBuilder.Length - 1);

		public void Add(string content)
		{
			this.stringBuilder.Append(' ', this.indentAmount);
			this.stringBuilder.AppendLine(content);
		}

		internal void AddWaitStatus<T>(ITestWaitCondition<T> condition, string description)
			=> this.AddWaitStatus(condition, description, this.Add);

		internal void AddWaitStatus<T>(
			ITestWaitCondition<T> condition,
			string description,
			IEnumerable<ITestOperationContext> children)
			=> this.AddWaitStatus(
				condition,
				description,
				fullDescription => this.Add(fullDescription, children));

		internal void AddWaitStatus<T>(
			ITestWaitCondition<T> condition,
			string description,
			Action<ContextStringBuilder> extraContext)
			=> this.AddWaitStatus(
				condition,
				description,
				fullDescription => this.AddWithNested(fullDescription, extraContext));

		private void AddWaitStatus<T>(
			ITestWaitCondition<T> condition,
			string description,
			Action<string> addFullContextFromDescription)
		{
			var completionTime = this.WaitContext.ElapsedTimeIfCompleted(condition);
			var hasCompleted = completionTime != null;
			var fullDescription = hasCompleted
				? $"{CompletedString(description)} (Completed in: {completionTime})"
				: this.WaitContext.HasStarted(condition)
					? WaitingString(description)
					: description;
			if (hasCompleted)
			{
				this.Add(fullDescription);
			}
			else
			{
				addFullContextFromDescription(fullDescription);
			}
		}

		internal void AddResponderStatus<T>(
			ITestResponder<T> responder,
			string description,
			Action<ContextStringBuilder> buildFullNestedContext)
		{
			var instruction = this.WaitContext.RelatedContexts(responder).FirstOrDefault() as ITestInstruction<T>;
			var fullyCompleted = instruction != null && this.RunContext.HasCompleted(instruction);
			if (fullyCompleted)
			{
				this.Add(CompletedString(description));
			}
			else
			{
				var fullDescription = this.WaitContext.HasStarted(responder)
					? WaitingString(description)
					: description;
				this.AddWithNested(fullDescription, buildFullNestedContext);
			}
		}

		internal void AddInstructionStatus<T>(
			ITestInstruction<T> instruction,
			SourceContext sourceContext,
			string description,
			[CanBeNull] ITestOperationContext child = null)
		{
			var e = this.RunContext.ErrorIfFailed(instruction);
			var fullDescription = e != null
				? ErrorString(description)
				: this.RunContext.HasCompleted(instruction)
					? CompletedString(description)
					: NotCompletedString(description);
			this.AddWithNested(
				fullDescription,
				b =>
				{
					child?.BuildFailureContext(this);

					if (e != null)
					{
						this.Add($"FAILED WITH: {e.GetType().Name}");
					}

					this.Add(sourceContext.ToString());
				});
		}

		public void AddWithNested(string content, string nestedContent) =>
			this.AddWithNested(content, b => b.Add(nestedContent));

		private void AddWithNested(string content, [CanBeNull] Action<ContextStringBuilder> contextAdder)
		{
			this.Add(content);
			if (contextAdder != null)
			{
				this.indentAmount += IndentChars;
				contextAdder(this);
				this.indentAmount -= IndentChars;
			}
		}

		public void NotAvailable() => this.Add(this.notAvailableText);

		public void Add(string content, ITestOperationContext child)
			=> this.Add(content, new[] { child });

		public void AddOptional(string description, IEnumerable<ITestOperationContext> children)
		{
			var childList = children.ToList();
			if (childList.Count > 0)
			{
				this.Add(description, childList);
			}
			else
			{
				this.Add(description + " ...");
			}
		}

		public void Add(string content, IEnumerable<ITestOperationContext> children)
		{
			this.Add(content);
			this.indentAmount += IndentChars;
			foreach (var child in children)
			{
				this.contextBuilder(child, this);
			}

			this.indentAmount -= IndentChars;
		}

		private void AddRoot(ITestOperationContext root)
		{
			this.contextBuilder(root, this);
		}

		private static string NotCompletedString(string description) => $"[ ] {description}";
		private static string CompletedString(string description) => $"[✓] {description}";
		private static string WaitingString(string description) => $"[.] {description}";
		private static string ErrorString(string description) => $"[!] {description}";
	}*/
}