using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Gherkin;
using ResponsibleGherkin.Generators;

namespace ResponsibleGherkin;

public static class Program
{
	private const string CommandName = "responsible-gherkin";

	private static readonly string[] DefaultConfig =
	{
		"# responsible-flavor: Unity",
		"# responsible-indent: 1 tab",
		"# responsible-namespace: MyNamespace",
		"# responsible-base-class: MyTestBase",
		"# responsible-executor: Executor",
	};

	private static readonly string FlavorTypes =
		string.Join(", ", Enum.GetValues(typeof(FlavorType)).Cast<FlavorType>());

	private static readonly string Description =
		"Generate Responsible test case stubs from Gherkin specifications." +
		"\n\n" +
		"The configuration can be specified either in comments in the Gherkin file, or in a separate file. " +
		"Comments in a Gherkin file always take precedence over the configuration file. " +
		"However, specifying a configuration value more than once in one source is an error. " +
		"The configuration format is fairly self-explanatory. " +
		"Here is the default configuration:" +
		"\n" +
		string.Join(Environment.NewLine, DefaultConfig) +
		"\n\n" +
		$"Valid flavors are: {FlavorTypes} (case insensitive)\n" +
		"Valid indent values are e.g. '1 tab', '4 spaces', '1 space'.\n" +
		"The namespace is the namespace for the generated code.\n" +
		"The base class is what the generated test classes will inherit from.\n" +
		"The executor is the property (or field) name in the base class, containing a TestInstructionExecutor.\n" +
		"\n" +
		"If no configuration file is specified, the default configuration is used.";

	// This only binds the root command invocation to the real file system and console.
	// I.e. deals only with non-testable parts.
	[ExcludeFromCodeCoverage]
	public static int Main(string[] args) =>
		BuildRootCommand(new FileSystem())
			.Invoke(args, new SystemConsole());

	public static RootCommand BuildRootCommand(IFileSystem fileSystem)
	{
		var inputFileArgument = new Argument<string>(
			"input",
			"Input feature file to generate code from");

		var outputDirectoryArgument = new Argument<string>(
			"output",
			() => ".",
			"Directory to write content into");

		var configFileOption = new Option<string>(
			new[] { "-c", "--config-file" },
			"Path to configuration file, otherwise a default configuration will be used");

		var forceOption = new Option<bool>(
			new[] { "-f", "--force" },
			() => false,
			"Overwrite output file, even if it already exists");

		var command = new RootCommand(Description)
		{
			configFileOption,
			forceOption,
			inputFileArgument,
			outputDirectoryArgument,
		};

		command.Name = CommandName;
		command.TreatUnmatchedTokensAsErrors = true;

		command.SetHandler((
				string? configFile,
				string inputFile,
				string outputDirectory,
				bool force,
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
					var baseConfig = Run("read configuration", () =>
					{
						var lines = configFile != null
							? fileSystem.File.ReadAllLines(configFile)
							: DefaultConfig;
						return CommentParser.ParseLines(lines);
					});

					var (feature, featureConfig) = Run("read input", () =>
					{
						using var fileReader = fileSystem.File.OpenText(inputFile);
						var document = new Parser().Parse(fileReader);
						return (document.Feature, CommentParser.Parse(document.Comments));
					});

					var configuration = Run("merge configurations", () =>
						Configuration.Merge(featureConfig, baseConfig));

					var content = Run("generate code", () => CodeGenerator.GenerateClass(
						feature,
						configuration));

					var _ = Run("write output", () =>
					{
						fileSystem.Directory.CreateDirectory(outputDirectory);
						var outputFileName = Path.Combine(outputDirectory, content.ClassFileName());

						if (!force && fileSystem.File.Exists(outputFileName))
						{
							throw new ArgumentException($"Output file '{outputFileName}' already exists.");
						}

						fileSystem.File.WriteAllLines(outputFileName, content.FileLines);

						invocationContext.Console.WriteLine($"Wrote file '{outputFileName}'");

						return 0; // Dummy value
					});

				}
				catch
				{
					invocationContext.ExitCode = 1;
				}
			},
			configFileOption,
			inputFileArgument,
			outputDirectoryArgument,
			forceOption);

		return command;
	}
}
