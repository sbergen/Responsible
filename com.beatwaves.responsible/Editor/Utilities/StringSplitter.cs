using System;
using System.Collections.Generic;
using System.Linq;

namespace Responsible.Editor.Utilities
{
	internal static class StringSplitter
	{
		/// <summary>
		/// Splits a string into substrings that are a maximum of <param name="maxLines" /> long.
		/// The last newline of each group is removed.
		/// </summary>
		public static IEnumerable<string> SubstringsOfMaxLines(this string str, int maxLines)
		{
			if (maxLines <= 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(maxLines), "Must be greater than zero");
			}

			if (str.Length == 0)
			{
				yield return str;
				yield break;
			}

			var yielded = 0;
			foreach (var nextIndex in str
				.AllIndicesOf('\n')
				.Where((_, i) => (i + 1) % maxLines == 0))
			{
				yield return str.Substring(yielded, nextIndex - yielded);
				yielded = nextIndex + 1;
			}

			if (yielded == 0)
			{
				yield return str;
			}
			else if (yielded < str.Length)
			{
				yield return str.Substring(yielded, str.Length - yielded);
			}
			else if (str.Last() == '\n')
			{
				yield return "";
			}
		}

		private static IEnumerable<int> AllIndicesOf(this string str, char ch)
		{
			var processed = 0;
			while ((processed = str.IndexOf(ch, processed)) != -1)
			{
				yield return processed;
				++processed;
			}
		}
	}
}
