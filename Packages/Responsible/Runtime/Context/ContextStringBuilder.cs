using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Responsible.Context
{
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

		internal static ContextStringBuilder MakeCompletedList(RunContext runContext, WaitContext waitContext)
		{
			var builder = new ContextStringBuilder(
				runContext,
				waitContext,
				DescriptionBuilder,
				DescriptionNotAvailable);
			foreach (var (context, elapsed) in waitContext.CompletedWaits)
			{
				builder.Add($"- Completed in {elapsed}", context);
			}

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

		public override string ToString() => this.stringBuilder.ToString();

		public void Add(string content)
		{
			this.stringBuilder.Append(' ', this.indentAmount);
			this.stringBuilder.AppendLine(content);
		}

		public string DescriptionForWait(ITestOperationContext context, string baseDescription)
		{
			var completionTime = this.WaitContext.ElapsedTimeIfCompleted(context);
			return completionTime != null
				? $"{baseDescription} (Completed in: {completionTime})"
				: this.WaitContext.HasStarted(context)
					? $"[...] {baseDescription}"
					: baseDescription;
		}

		internal void AddInstructionStatus<T>(
			ITestInstruction<T> instruction,
			SourceContext sourceContext,
			string description)
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
					if (e != null)
					{
						this.Add($"FAILED WITH: {e.GetType().Name}");
					}

					this.Add(sourceContext.ToString());
				});
		}

		public void AddWithNested(string content, string nestedContent) =>
			this.AddWithNested(content, b => b.Add(nestedContent));

		public void AddWithNested(string content, [CanBeNull] Action<ContextStringBuilder> contextAdder)
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
	}
}