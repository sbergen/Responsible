using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Responsible.Context
{
	internal readonly struct SourceContext
	{
		private enum Kind
		{
			NonCollapsible,
			Collapsible,
			Aggregate,
		}

		private static readonly Func<string, string> FormatSourcePath;

		private readonly Kind kind;
		public readonly IReadOnlyList<string> SourceLines;

		static SourceContext()
		{
#if UNITY_EDITOR
			var dataPath = UnityEngine.Application.dataPath;

			// Compensate for how Unity mangles the paths
			var projectPath = dataPath
				.Substring(0, dataPath.Length - "Assets".Length)
				.Replace("/", "\\");

			FormatSourcePath = path => path.StartsWith(projectPath)
				? path.Substring(projectPath.Length).Replace("\\", "/")
				: path;
#else
			FormatSourcePath = path => path;
#endif
		}

		public SourceContext(
			string operationName,
			string memberName,
			string sourceFilePath,
			int sourceLineNumber,
			bool isCollapsible = false)
		{
			this.SourceLines = new[] { Format(operationName, memberName, sourceFilePath, sourceLineNumber) };
			this.kind = isCollapsible ? Kind.Collapsible : Kind.NonCollapsible;
		}

		private SourceContext(SourceContext newContext, SourceContext oldContext)
		{
			this.SourceLines = newContext.SourceLines.Concat(oldContext.SourceLines).ToList();
			this.kind = Kind.Aggregate;
		}

		[Pure]
		public SourceContext Push(SourceContext newContext) =>
			newContext.kind == Kind.Collapsible && newContext.SourceLines[0] == this.SourceLines[0]
				? this
				: new SourceContext(newContext, this);

		[ExcludeFromCodeCoverage] // Used only for prettier debugger output
		public override string ToString() => string.Join("\n", this.SourceLines);

		private static string Format(string operationName, string memberName, string sourceFilePath, int sourceLineNumber)
			=> $"[{operationName}] {memberName} (at {FormatSourcePath(sourceFilePath)}:{sourceLineNumber})";
	}
}
