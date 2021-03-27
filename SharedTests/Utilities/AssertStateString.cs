using System;
using System.Text;
using NUnit.Framework;

namespace Responsible.Tests.Utilities
{
	/// <summary>
	/// Helper for doing assertions on state strings.
	/// </summary>
	public class AssertStateString
	{
		private readonly string str;
		private readonly StringBuilder assertedStrings = new StringBuilder();

		private int currentIndex;

		internal AssertStateString(string str)
		{
			this.str = str;
		}

		public AssertStateString NotStarted(string description) => this.DetailsWithMarker(' ', description);
		public AssertStateString Completed(string description) => this.DetailsWithMarker('✓', description);
		public AssertStateString Failed(string description) => this.DetailsWithMarker('!', description);
		public AssertStateString Waiting(string description) => this.DetailsWithMarker('.', description);

		public AssertStateString FailureDetails() => this.Details("Failed with:");

		public AssertStateString Nowhere(string details)
		{
			Assert.That(this.str, Does.Not.Contain(details));
			return this;
		}

		public AssertStateString Details(string text)
		{
			var markerIndex = this.str.IndexOf(text, this.currentIndex, StringComparison.Ordinal);

			if (markerIndex < 0)
			{
				var alreadyAsserted = string.Join("\n  ", this.assertedStrings);
				Assert.Fail(
$@"Expected to find
  {text}
After consuming
  {alreadyAsserted}
In:
{this.str}");
			}

			this.assertedStrings.Append(text);
			this.currentIndex = markerIndex + text.Length;
			return this;
		}

		private AssertStateString DetailsWithMarker(char marker, string description) =>
			this.Details($"[{marker}] {description}");
	}
}
