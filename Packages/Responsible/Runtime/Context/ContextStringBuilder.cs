using System;
using System.Collections.Generic;
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

		internal WaitContext WaitContext { get; }

		internal static ContextStringBuilder MakeDescription(
			ITestOperationContext rootContext,
			WaitContext waitContext)
		{
			var builder = new ContextStringBuilder(waitContext, DescriptionBuilder, DescriptionNotAvailable);
			builder.AddRoot(rootContext);
			return builder;
		}

		internal static ContextStringBuilder MakeFailureContext(
			ITestOperationContext rootContext,
			WaitContext waitContext)
		{
			var builder = new ContextStringBuilder(waitContext, FailureBuilder, FailureContextNotAvailable);
			builder.AddRoot(rootContext);
			return builder;
		}

		internal static ContextStringBuilder MakeCompletedList(WaitContext waitContext)
		{
			var builder = new ContextStringBuilder(waitContext, DescriptionBuilder, DescriptionNotAvailable);
			foreach (var (context, elapsed) in waitContext.CompletedWaits)
			{
				builder.Add($"- Completed in {elapsed}", context);
			}

			return builder;
		}

		private ContextStringBuilder(
			WaitContext waitContext,
			Action<ITestOperationContext, ContextStringBuilder> contextBuilder,
			string notAvailableText)
		{
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
	}
}