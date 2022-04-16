using Gherkin.Ast;
using ResponsibleGherkin.Utilities;

namespace ResponsibleGherkin.Generators;

public static class BackgroundGenerator
{
	public static IEnumerable<Line> Generate(Background? background, Flavor flavor, Configuration configuration)
	{
		if (background == null)
		{
			yield break;
		}

		foreach (var line in DescriptionGenerator.Generate(background))
		{
			yield return line;
		}

		var backgroundFlavor = flavor.BackgroundFlavor;
		if (backgroundFlavor.SetupAttribute != null)
		{
			yield return $"[{flavor.BackgroundFlavor.SetupAttribute}]";
		}

		var signature = $"public {flavor.ReturnType} {backgroundFlavor.SetupMethodName}()";
		yield return $"{signature} => this.{configuration.ExecutorName}.{flavor.RunMethod}(";

		yield return GenerateScenario(background).IndentBy(1);

		foreach (var line in StepsGenerator.Generate(background))
		{
			yield return line.IndentBy(1);
		}

		if (backgroundFlavor.TearDownMethodName != null)
		{
			yield return "";
			yield return $"public {flavor.ReturnType} {backgroundFlavor.TearDownMethodName}() => Task.CompletedTask;";
		}

		yield return "";
	}

	private static Line GenerateScenario(Background background) => $"Scenario({background.Name.Quote()}),";
}
