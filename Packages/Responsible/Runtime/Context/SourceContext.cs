using System.Collections.Generic;
using System.Linq;

namespace Responsible.Context
{
	internal readonly struct SourceContext
	{
		private readonly IReadOnlyList<string> sourceLines;

		internal SourceContext(string sourceFilePath, int sourceLineNumber)
		{
			this.sourceLines = new[] { Format(sourceFilePath, sourceLineNumber) };
		}

		private SourceContext(SourceContext parent, SourceContext child)
		{
			this.sourceLines = parent.sourceLines.Concat(child.sourceLines).ToList();
		}

		internal SourceContext Append(SourceContext other) => new SourceContext(other, this);

		public override string ToString() => string.Join("\n", this.sourceLines);

		private static string Format(string sourceFilePath, int sourceLineNumber)
			=> $"{sourceFilePath}:{sourceLineNumber}";
	}
}