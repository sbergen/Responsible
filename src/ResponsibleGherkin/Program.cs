using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json;
using Gherkin;

namespace ResponsibleGherkin;

public static class Program
{
	// This only binds the root command invocation to the real file system and console.
	// I.e. deals only with non-testable parts.
	[ExcludeFromCodeCoverage]
	public static int Main(string[] args) =>
		BuildRootCommand(new FileSystem())
			.Invoke(args, new SystemConsole());

	public static RootCommand BuildRootCommand(IFileSystem fileSystem)
	{
		var rootCommand = new RootCommand(
			"Generate Responsible test case stubs from Gherkin specifications.")
		{
			ConfigureCommand(),
			GenerateCommand(fileSystem),
		};

		rootCommand.Name = "responsible-gherkin";
		rootCommand.TreatUnmatchedTokensAsErrors = true;

		return rootCommand;
	}

	private static Command ConfigureCommand()
	{
		var flavorOption = new Option<FlavorType>(
			new[] { "--flavor", "-f" },
			"Flavor of code to generate.");

		var command = new Command("configure", "Generate configuration file");
		return command;
	}

	private static Command GenerateCommand(IFileSystem fileSystem)
	{
		var configFileArgument = new Argument<string>(
			"config",
			"Path to configuration file to use.");

		var inputFileArgument = new Argument<string>(
			"input",
			"Input feature file to generate code from.");

		var outputDirectoryArgument = new Argument<string>(
			"output",
			"Directory to write content into.");

		var generateCommand = new Command("generate", "Generate test case stubs")
		{
			configFileArgument,
			inputFileArgument,
			outputDirectoryArgument,
		};

		generateCommand.SetHandler(
			(
				string configFile,
				string inputFile,
				string outputDirectory,
				InvocationContext invocationContext) =>
			{
				T Run<T>(string name, Func<T> operation)
				{
					try
					{
						return operation();
					}
					catch (Exception exception)
					{
						invocationContext.Console.Error.WriteLine($"Could not {name}: {exception.Message}");
						throw;
					}
				}

				try
				{
					var configuration = Run("read configuration", () =>
					{
						using var fileReader = fileSystem.File.OpenRead(configFile);
						return JsonSerializer.Deserialize<Configuration>(fileReader);
					});

					var feature = Run("read input", () =>
					{
						using var fileReader = fileSystem.File.OpenText(inputFile);
						var document = new Parser { StopAtFirstError = true }.Parse(fileReader);
						return document.Feature;
					});

					var content = Run("generate content", () => CodeGenerator.GenerateClass(
						feature,
						configuration));

					var _ = Run("write output", () =>
					{
						fileSystem.File.WriteAllLines(
							Path.Combine(outputDirectory, content.ClassFileName()),
							content.FileLines);
						return 0;
					});

				}
				catch
				{
					invocationContext.ExitCode = 1;
				}
			},
			configFileArgument,
			inputFileArgument,
			outputDirectoryArgument);

		return generateCommand;
	}
}
