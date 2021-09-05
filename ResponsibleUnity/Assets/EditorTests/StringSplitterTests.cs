using System.Linq;
using NUnit.Framework;
using Responsible.Editor.Utilities;

namespace Responsible.EditorTests
{
	public class StringSplitterTests
	{
		[TestCase(1, "", ExpectedResult = new object[] { "" })]
		[TestCase(1, "\n", ExpectedResult = new object[] { "", "" })]
		[TestCase(1, "foo", ExpectedResult = new object[] { "foo" })]
		[TestCase(1, "foo\n", ExpectedResult = new object[] { "foo", "" })]
		[TestCase(1, "foo\n\n", ExpectedResult = new object[] { "foo", "", "" })]
		[TestCase(1, "foo\nbar", ExpectedResult = new object[] { "foo", "bar" })]
		[TestCase(1, "foo\nbar\n", ExpectedResult = new object[] { "foo", "bar", "" })]
		[TestCase(2, "", ExpectedResult = new object[] { "" })]
		[TestCase(2, "foo", ExpectedResult = new object[] { "foo" })]
		[TestCase(2, "\n", ExpectedResult = new object[] { "\n" })]
		[TestCase(2, "\n\n", ExpectedResult = new object[] { "\n", "" })]
		[TestCase(2, "foo\n", ExpectedResult = new object[] { "foo\n" })]
		[TestCase(2, "foo\n\n", ExpectedResult = new object[] { "foo\n", "" })]
		[TestCase(2, "foo\n\n\n", ExpectedResult = new object[] {  "foo\n", "\n" })]
		[TestCase(2, "foo\n\n\n\n", ExpectedResult = new object[] {  "foo\n", "\n", "" })]
		public string[] SubstringsOfMaxLines(int maxLines, string input) =>
	        input.SubstringsOfMaxLines(maxLines).ToArray();

		[Test]
		public void SubstringsOfMaxLines_ReturnsInputAsReference_WhenNotSplit()
		{
			var input = "foo\n";
			Assert.AreSame(input, input.SubstringsOfMaxLines(2).Single());
		}

	}
}
