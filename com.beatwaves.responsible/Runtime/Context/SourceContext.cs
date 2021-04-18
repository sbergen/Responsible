using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Responsible.Context
{
	internal readonly struct SourceContext
	{
		private static readonly Func<string, string> FormatSourcePath;

		public readonly IReadOnlyList<string> SourceLines;

		static SourceContext()
		{
#if UNITY_EDITOR
			var dataPath = UnityEngine.Application.dataPath;
			var projectPath = dataPath.Substring(0, dataPath.Length - "Assets".Length);
			FormatSourcePath = path => path.StartsWith(projectPath)
				? path.Substring(projectPath.Length)
				: path;
#else
			FormatSourcePath = path => path;
#endif
		}

		internal SourceContext(string operationName, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			this.SourceLines = new[] { Format(operationName, memberName, sourceFilePath, sourceLineNumber) };
		}

		private SourceContext(SourceContext parent, SourceContext child)
		{
			this.SourceLines = parent.SourceLines.Concat(child.SourceLines).ToList();
		}

		[Pure]
		internal SourceContext Append(SourceContext other) => new SourceContext(other, this);

		[ExcludeFromCodeCoverage] // Used only for prettier debugger output
		public override string ToString() => string.Join("\n", this.SourceLines);

		private static string Format(string operationName, string memberName, string sourceFilePath, int sourceLineNumber)
			=> $"[{operationName}] {memberName} (at {FormatSourcePath(sourceFilePath)}:{sourceLineNumber})";
	}
}
