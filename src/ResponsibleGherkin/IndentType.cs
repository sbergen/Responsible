using System.Text.Json.Serialization;

namespace ResponsibleGherkin;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndentType
{
	Tabs,
	Spaces,
}
