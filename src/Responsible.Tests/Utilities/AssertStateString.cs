using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Responsible.Tests.Utilities
{
	/// <summary>
	/// Helper for doing assertions on state strings.
	/// </summary>
	public class AssertStateString
	{
		private readonly string str;
		private readonly List<string> assertedStrings = new List<string>();

		private int currentIndex;

		internal AssertStateString(string str)
		{
			this.str = str;
		}

		public AssertStateString NotStarted(string description) => this.DetailsWithMarker(' ', description);
		public AssertStateString Completed(string description) => this.DetailsWithMarker('✓', description);
		public AssertStateString Failed(string description) => this.DetailsWithMarker('!', description);
		public AssertStateString Waiting(string description) => this.DetailsWithMarker('.', description);
		public AssertStateString Canceled(string description) => this.DetailsWithMarker('-', description);

		public AssertStateString FailureDetails() => this.Details("Failed with:");

		// An empty line requires whitespace to work nicely with Unity
		public AssertStateString EmptyLine() => this.DetailsRegex(@"\n\s+\n");

		public AssertStateString Details(string details)
		{
			var index = this.str.Substring(this.currentIndex).IndexOf(details, StringComparison.Ordinal);

			if (index < 0)
			{
				FailWithNoMatch(details);
			}

			this.assertedStrings.Add(details);
			this.currentIndex += index + details.Length;
			return this;
		}

		public AssertStateString DetailsRegex([RegexPattern] string regex)
		{
			var match = Regex.Match(this.str.Substring(this.currentIndex), regex);

			if (!match.Success)
			{
				FailWithNoMatch(regex);
			}

			this.assertedStrings.Add(regex);
			this.currentIndex += match.Index + match.Value.Length;
			return this;
		}

		private void FailWithNoMatch(string toMatch)
		{
			var alreadyAsserted = string.Join("\n  ", this.assertedStrings);
				Assert.Fail(
$@"Expected to find
  {toMatch}
After consuming
  {alreadyAsserted}
In:
{this.str}");
		}

		private AssertStateString DetailsWithMarker(char marker, string description) =>
			this.Details($@"[{marker}] {description}");
	}
}
