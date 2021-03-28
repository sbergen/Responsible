using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Responsible.Context
{
	internal readonly struct SourceContext
	{
		private static readonly int StripFromPaths = 0;

		public readonly IReadOnlyList<string> SourceLines;

		static SourceContext()
		{
			// If this were to be run in a player, using these paths would not be reliable.
			// Could be fixed with some build processor maybe, but is it worth it?
#if UNITY_EDITOR
			StripFromPaths = UnityEngine.Application.dataPath.Length - "Assets".Length;
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
			=> $"[{operationName}] {memberName} (at {sourceFilePath.Substring(StripFromPaths)}:{sourceLineNumber})";
	}
}
