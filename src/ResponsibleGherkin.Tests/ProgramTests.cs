using System.CommandLine;
using System.CommandLine.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using NUnit.Framework;
using static VerifyNUnit.Verifier;

namespace ResponsibleGherkin.Tests;

public class ProgramTests
{
	private MockFileSystem fileSystem;
	private TestConsole console;

	[SetUp]
	public void SetUp()
	{
		this.fileSystem = new MockFileSystem();
		this.console = new TestConsole();
	}

	[Test]
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
