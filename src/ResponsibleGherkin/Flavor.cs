namespace ResponsibleGherkin;

public record Flavor(
	string[] RequiredNamespaces,
	string TestAttribute,
	string ReturnType,
	string RunMethod)
{
	public static readonly Flavor Unity = new(
		new[]
		{
			"System.Collections",
			"UnityEngine.TestTools",
		},
		"UnityTest",
		"IEnumerator",
		"YieldScenario");

	public static readonly Flavor NUnit = new(
		new[]
		{
			"System.Threading.Tasks",
			"NUnit.Framework",
		},
		"Test",
		"Task",
		"RunScenario");

	public static readonly Flavor Xunit = new(
		new[]
		{
			"System.Threading.Tasks",
			"Xunit",
		},
		"Fact",
		"Task",
		"RunScenario");

	public static Flavor FromType(FlavorType type) => type switch
	{
		FlavorType.Unity => Unity,
		FlavorType.NUnit => NUnit,
		FlavorType.Xunit => Xunit,
		_ => throw new ArgumentOutOfRangeException(
			nameof(type), type, "Invalid flavor of code generation"),
	};
}
