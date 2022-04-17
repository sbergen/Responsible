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

	// This only binds the root command invocation to the real file system and console.
	// I.e. deals only with non-testable parts.
	[ExcludeFromCodeCoverage]
	public static int Main(string[] args) =>
		BuildRootCommand(new FileSystem())
			.Invoke(args, new SystemConsole());

	public static RootCommand BuildRootCommand(IFileSystem fileSystem)
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

		var command = new RootCommand("Generate Responsible test case stubs from Gherkin specifications.")
		{
			configFileArgument,
			inputFileArgument,
			outputDirectoryArgument,
		};

		command.Name = CommandName;
		command.TreatUnmatchedTokensAsErrors = true;

		command.SetHandler((
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
					var baseConfig = Run("read configuration", () =>
					{
						var lines = fileSystem.File.ReadAllLines(configFile);
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

		return command;
	}
}
