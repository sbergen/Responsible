using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using ResponsibleGherkin.Generators;

namespace ResponsibleGherkin;

public static class Program
{
	private const string CommandName = "responsible-gherkin";

	private static readonly string[] DefaultConfig =
	{
		"# responsible-flavor: Unity",
		"# responsible-indent: 4 spaces",
		"# responsible-namespace: MyNamespace",
		"# responsible-base-class: MyTestBase",
		"# responsible-executor: Executor",
	};

	private static readonly string FlavorTypes =
		string.Join(", ", Enum.GetValues(typeof(FlavorType)).Cast<FlavorType>());

	private static readonly string Description =
		"Generate Responsible test case stubs from Gherkin specifications." +
		"\n\n" +
		"The configuration will be resolved by merging the following sources in descending priority order:\n" +
		"  * Comments in the Gherkin file\n" +
		"  * Manually specified configuration file\n" +
		"  * EditorConfig file resolved from the output path\n" +
		"  * Only if no manual configuration provided, the default configuration\n" +
		"\n" +
		"After merging the configurations, there must be a value specified for each configuration property. " +
		"However, specifying a configuration value more than once in one source is an error. " +
		"\n\n" +
		"The configuration format is fairly self-explanatory. " +
		"Here is the default configuration:" +
		"\n" +
		string.Join(Environment.NewLine, DefaultConfig) +
		"\n\n" +
		"The properties are\n" +
		$"  * flavor: {FlavorTypes} (case insensitive)\n" +
		"  * indent: e.g. '1 tab', '4 spaces', '1 space'\n" +
		"  * namespace: the namespace for the generated code\n" +
		"  * base-class: base class for generated test classes\n" +
		"  * executor: property (or field) name in the base class, containing a TestInstructionExecutor";

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
					var providedConfig = Run("read configuration", () =>
						configFile != null
							? CommentParser.ParseLines(fileSystem.File.ReadAllLines(configFile))
							: PartialConfiguration.Empty);

					// Only use default as a fallback if no config file provided
					var fallbackConfig = configFile != null
						? PartialConfiguration.Empty
						: CommentParser.ParseLines(DefaultConfig);

					var input = Run("read input", () => InputFile.Read(fileSystem, inputFile));

					var configuration = Run("merge configurations", () =>
						Configuration.Merge(
							input.CommentsConfiguration,
							providedConfig,
							input.EditorConfigConfiguration,
							fallbackConfig));

					var generatedClass = Run("generate code", () => CodeGenerator
						.GenerateClass(input.Feature, configuration));

					var _ = Run("write output", () =>
					{
						fileSystem.Directory.CreateDirectory(outputDirectory);
						var outputFileName = Path.Combine(outputDirectory, input.ClassFileName);

						if (!force && fileSystem.File.Exists(outputFileName))
						{
							throw new ArgumentException($"Output file '{outputFileName}' already exists.");
						}

						fileSystem.File.WriteAllLines(outputFileName, generatedClass.FileLines);

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
