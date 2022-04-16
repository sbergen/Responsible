using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json;
using Gherkin;
using ResponsibleGherkin.Generators;

namespace ResponsibleGherkin;

public static class Program
{
	private const string CommandName = "responsible-gherkin";
	private const string ConfigureName = "configure";
	private const string GenerateName = "generate";

	private const string MainDescription =
		$@"Generate Responsible test case stubs from Gherkin specifications.

First generate a configuration using
    {CommandName} {ConfigureName} ...

store it to a file, and then generate your code using
    {CommandName} {GenerateName} ...
";

	// This only binds the root command invocation to the real file system and console.
	// I.e. deals only with non-testable parts.
	[ExcludeFromCodeCoverage]
	public static int Main(string[] args) =>
		BuildRootCommand(new FileSystem())
			.Invoke(args, new SystemConsole());

	public static RootCommand BuildRootCommand(IFileSystem fileSystem)
	{
		var rootCommand = new RootCommand(MainDescription)
		{
			ConfigureCommand(),
			GenerateCommand(fileSystem),
		};

		rootCommand.Name = CommandName;
		rootCommand.TreatUnmatchedTokensAsErrors = true;

		return rootCommand;
	}

	private static Command ConfigureCommand()
	{
		var flavorArgument = new Argument<FlavorType>(
			"flavor",
			"Flavor of code generation");

		var namespaceArgument = new Argument<string>(
			"namespace",
			"Namespace to put classes in");

		var baseClassArgument = new Argument<string>(
			"base-class",
			"Base class to derive from");

		var executorArgument = new Argument<string>(
			"executor",
			"Name of parameter that has the TestInstructionExecutor");

		var indentTypeArgument = new Argument<IndentType>(
			"indent-type",
			"Type of indentation to use");

		var indentAmountArgument = new Argument<int>(
			"indent-amount",
			"Amount of indentation to use");

		var command = new Command(
			ConfigureName,
			$"Generate a configuration to be used with {GenerateName}")
		{
			flavorArgument,
			namespaceArgument,
			baseClassArgument,
			executorArgument,
			indentTypeArgument,
			indentAmountArgument,
		};

		command.SetHandler((
				FlavorType flavor,
				string @namespace,
				string baseClass,
				string executorName,
				IndentType indentType,
				int indentAmount,
				InvocationContext invocationContext) =>
			{
				var configuration = new Configuration(
					flavor,
					new IndentInfo(indentAmount, indentType),
					@namespace,
					baseClass,
					executorName);

				invocationContext.Console.Out.WriteLine(JsonSerializer.Serialize(
					configuration, new JsonSerializerOptions { WriteIndented = true }));
			},
			flavorArgument,
			namespaceArgument,
			baseClassArgument,
			executorArgument,
			indentTypeArgument,
			indentAmountArgument);

		return command;
	}

	private static Command GenerateCommand(IFileSystem fileSystem)
	{
		var configFileArgument = new Argument<string>(
			"config",
			"Path to configuration file to use");

		var inputFileArgument = new Argument<string>(
			"input",
			"Input feature file to generate code from");

		var outputDirectoryArgument = new Argument<string>(
			"output",
			"Directory to write content into");

		var generateCommand = new Command(GenerateName, "Generate test case stubs")
		{
			configFileArgument,
			inputFileArgument,
			outputDirectoryArgument,
		};

		generateCommand.SetHandler((
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
						var document = new Parser().Parse(fileReader);
						return document.Feature;
					});

					var content = Run("generate code", () => CodeGenerator.GenerateClass(
						feature,
						configuration));

					var _ = Run("write output", () =>
					{
						fileSystem.Directory.CreateDirectory(outputDirectory);
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
