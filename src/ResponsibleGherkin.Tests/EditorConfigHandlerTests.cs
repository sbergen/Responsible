using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class EditorConfigHandlerTests
{
	[Fact]
	public void NullProperties_ReturnsEmptyConfiguration() =>
		default(Dictionary<string, string>).ConfigFromEditorConfigProperties()
			.Should().BeEquivalentTo(PartialConfiguration.Empty);

	[Fact]
	public void EmptyProperties_ReturnsEmptyConfiguration() =>
		new Dictionary<string, string>().ConfigFromEditorConfigProperties()
			.Should().BeEquivalentTo(PartialConfiguration.Empty);

	[Fact]
	public void TabIndentStyle_ReturnsTabs() => new Dictionary<string, string>
		{
			["indent_style"] = "tab",
			["indent_size"] = "8", // This should be ignored
		}
		.ConfigFromEditorConfigProperties()
		.Should().BeEquivalentTo(Config(IndentInfo.Tabs));

	[Fact]
	public void SpaceIndentStyle_ReturnsEmpty_WhenTabUsedWithoutTabWidth() => new Dictionary<string, string>
		{
			["indent_style"] = "space",
			["indent_size"] = "tab",
		}
		.ConfigFromEditorConfigProperties()
		.Should().BeEquivalentTo(PartialConfiguration.Empty);

	[Fact]
	public void SpaceIndentStyle_ReturnsTabWidth_WhenTabSpecified() => new Dictionary<string, string>
		{
			["indent_style"] = "space",
			["indent_size"] = "tab",
			["tab_width"] = "3",
		}
		.ConfigFromEditorConfigProperties()
		.Should().BeEquivalentTo(Config(new IndentInfo(3, ' ')));

	[Fact]
	public void SpaceIndentStyle_ReturnsIndentSize_WhenIntegerSpecified() => new Dictionary<string, string>
		{
			["indent_style"] = "space",
			["indent_size"] = "3",
		}
		.ConfigFromEditorConfigProperties()
		.Should().BeEquivalentTo(Config(new IndentInfo(3, ' ')));

	// Get some coverage, but this is mostly just third party code, so no need to test the details.
	[Fact]
	public void ResolveEditorConfigProperties_ReturnsNonEmptyForThisFile() =>
		EditorConfigHandler.ResolveEditorConfigProperties(ThisFileName()).Should().NotBeEmpty();

	private static PartialConfiguration Config(IndentInfo? indentInfo) => new(IndentInfo: indentInfo);

	private static string ThisFileName([CallerFilePath] string path = "") => path;
}
