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
	public Task Verify_OutputWithNoArguments()
	{
		this.Run();
		return Verify(this.Output());
	}

	private void Run(params string[] args) => Program
		.BuildRootCommand(this.fileSystem)
		.Invoke(args, this.console);

	private string Output() => this.console.Out.ToString()!;
}
