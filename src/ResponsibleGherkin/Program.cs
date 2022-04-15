using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace ResponsibleGherkin;

public static class Program
{
	// This only binds the root command invocation to the real file system and console.
	// I.e. deals only with non-testable parts.
	[ExcludeFromCodeCoverage]
	public static void Main(string[] args) =>
		BuildRootCommand(new FileSystem())
			.Invoke(args, new SystemConsole());

	public static RootCommand BuildRootCommand(IFileSystem fileSystem)
	{
		var inputFileArgument = new Argument<string>(
			"input-file",
			"Input feature file to generate code from.");

		var rootCommand = new RootCommand("Generate Responsible test case stubs from Gherkin specifications.")
		{
			inputFileArgument,
		};

		rootCommand.Name = "responsible-gherkin";

		return rootCommand;
	}
}
