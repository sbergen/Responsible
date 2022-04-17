using Gherkin.Ast;

namespace ResponsibleGherkin.Utilities;

public static class LocationExtensions
{
	public static string Diagnostics(this Location location) => $"(at {location.Line}:{location.Column})";
}
