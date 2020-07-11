using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Responsible.Context
{
	public readonly struct SourceContext
	{
		private static readonly int StripFromPaths;
		private readonly IReadOnlyList<string> sourceLines;

		static SourceContext()
		{
			// If this were to be run in a player, using these paths would not be reliable.
			// Could be fixed with some build processor maybe, but is it worth it?
#if UNITY_EDITOR
			StripFromPaths = Application.dataPath.Length - "Assets".Length;
#endif
		}

		internal SourceContext(string memberName, string sourceFilePath, int sourceLineNumber)
		{
			this.sourceLines = new[] { Format(memberName, sourceFilePath, sourceLineNumber) };
		}

		private SourceContext(SourceContext parent, SourceContext child)
		{
			this.sourceLines = parent.sourceLines.Concat(child.sourceLines).ToList();
		}

		internal SourceContext Append(SourceContext other) => new SourceContext(other, this);

		public override string ToString() => string.Join("\n", this.sourceLines);

		private static string Format(string memberName, string sourceFilePath, int sourceLineNumber)
			=> $"{memberName} (at {sourceFilePath.Substring(StripFromPaths)}:{sourceLineNumber})";
	}
}