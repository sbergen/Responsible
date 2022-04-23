using EditorConfig.Core;

namespace ResponsibleGherkin;

public static class EditorConfigHandler
{
	// Make this separate for better testability
	public static IReadOnlyDictionary<string, string>? ResolveEditorConfigProperties(string filePath) =>
		new EditorConfigParser().Parse(filePath)?.Properties;

	public static PartialConfiguration ConfigFromEditorConfigProperties(
		this IReadOnlyDictionary<string, string>? properties) => properties != null
		? new PartialConfiguration(IndentInfo: ResolveIndentStyle(properties))
		: PartialConfiguration.Empty;

	private static IndentInfo? ResolveIndentStyle(IReadOnlyDictionary<string, string> properties) =>
		properties.GetValueOrDefault("indent_style") switch
		{
			"tab" => IndentInfo.Tabs,
			"space" => ResolveSpaceIndentStyle(properties),
			_ => null,
		};

	private static IndentInfo? ResolveSpaceIndentStyle(IReadOnlyDictionary<string, string> properties) =>
		properties.GetValueOrDefault("indent_size") switch
		{
			"tab" => properties.GetValueOrDefault("tab_width")?.ParseSpaces(),
			{} amount => ParseSpaces(amount),
			_ => null,
		};

	private static IndentInfo? ParseSpaces(this string amountStr) => int.TryParse(amountStr, out var result)
		? new IndentInfo(result, ' ')
		: null;
}
