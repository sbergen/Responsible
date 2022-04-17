namespace ResponsibleGherkin;

public record Flavor(
	BackgroundFlavor BackgroundFlavor,
	string[] RequiredNamespaces,
	string TestAttribute,
	string ReturnType,
	string RunMethod)
{
	public static readonly Flavor Unity = new(
		new BackgroundFlavor(
			"SetUpBackground",
			"UnitySetUp"),
		new[]
		{
			"System.Collections",
			"UnityEngine.TestTools",
		},
		"UnityTest",
		"IEnumerator",
		"YieldScenario");

	public static readonly Flavor NUnit = new(
		new BackgroundFlavor(
			"SetUpBackground",
			"SetUp"),
		new[]
		{
			"System.Threading.Tasks",
			"NUnit.Framework",
		},
		"Test",
		"Task",
		"RunScenario");

	public static readonly Flavor Xunit = new(
		new BackgroundFlavor(
			"InitializeAsync",
			null,
			"IAsyncLifetime",
			"DisposeAsync"),
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
