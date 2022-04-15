using System.CommandLine;
using System.CommandLine.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using static VerifyXunit.Verifier;

namespace ResponsibleGherkin.Tests;

[UsesVerify]
public class ProgramTests
{
	private readonly MockFileSystem fileSystem = new();
	private readonly TestConsole console = new();

	[Fact]
	public Task Verify_Output_WhenNoArguments()
	{
		this.Run();
		return Verify(this.ConsoleOutput());
	}

	[Fact]
	public Task Verify_Output_WhenInputFileDoesNotExist()
	{
		this.Run("fake-file");
		return Verify(this.ConsoleOutput());
	}

	private void Run(params string[] args) => Program
		.BuildRootCommand(this.fileSystem)
		.Invoke(args, this.console);

	private object ConsoleOutput() => new
	{
		Out = this.console.Out.ToString(),
		Error = this.console.Error.ToString(),
	};
}
