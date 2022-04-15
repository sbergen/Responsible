using System;
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

	public ProgramTests()
	{
		this.fileSystem.AddFile(
			"config.json",
			new MockFileData(TestFeatures.DefaultConfigurationJson));

		this.fileSystem.AddFile(
			"MinimalFeature.feature",
			TestFeatures.MinimalFeatureString);

		this.fileSystem.AddFile(
			"InvalidFeature.feature",
			new MockFileData("foobar"));
	}

	[Fact]
	public async Task Verify_Output_WhenNoArguments()
	{
		this.RunAssertingFailure();
		await Verify(this.ConsoleOutput());
	}

	[Theory]
	[InlineData("configure")]
	[InlineData("generate")]
	public async Task Verify_CommandHelp(string command)
	{
		this.RunAssertingSuccess(command, "-h");
		await Verify(this.ConsoleOutput()).UseParameters(command);
	}

	[Fact]
	public async Task Verify_SuccessfulConfigure()
	{
		this.RunAssertingSuccess("configure", "xunit", "MyNamespace", "BddTest", "Executor", "tabs", "1");
		await Verify(this.ConsoleOutput());
	}

	[Fact]
	public void Generate_ProducesSameResultAsManualInvoke()
	{
		var expected = CodeGenerator.GenerateClass(
				TestFeatures.LoadFeature("MinimalFeature").Feature,
				TestFeatures.DefaultConfiguration)
			.BuildFileContent();

		this.RunAssertingSuccess("generate", "config.json", "MinimalFeature.feature", "./");
		var generatedContent = this.fileSystem.File.ReadAllText("MinimalFeature.cs");

		Assert.Equal(expected, generatedContent);
	}

	[Fact]
	public void Generate_ContainsDescriptiveError_WhenConfigFileIsMissing()
	{
		this.RunAssertingFailure("generate", "foobar", "MinimalFeature.feature", "./");
		Assert.Contains("read configuration", this.console.Error.ToString()!);
	}

	[Fact]
	public void Generate_ContainsDescriptiveError_WhenInputFileIsMissing()
	{
		this.RunAssertingFailure("generate", "config.json", "foobar", "./");
		Assert.Contains("read input", this.console.Error.ToString()!);
	}

	[Fact]
	public void Generate_ContainsDescriptiveError_WhenInputFileIsInvalid()
	{
		this.RunAssertingFailure("generate", "config.json", "InvalidFeature.feature", "./");
		Assert.Contains("read input", this.console.Error.ToString()!);
	}

	[Fact]
	public void Generate_ContainsDescriptiveError_WhenDirectoryIsInvalid()
	{
		this.RunAssertingFailure("generate", "config.json", "MinimalFeature.feature", "......");
		Assert.Contains("write output", this.console.Error.ToString()!);
	}

	private void RunAssertingFailure(params string[] args)
	{
		var result = this.Run(args);
		Assert.True(result != 0, $"Run should fail, output was {this.console.Out}");
	}

	private void RunAssertingSuccess(params string[] args)
	{
		var result = this.Run(args);
		Assert.True(result == 0, $"Run should succeed, failed with {this.console.Error}");
	}

	private int Run(params string[] args) => Program
		.BuildRootCommand(this.fileSystem)
		.Invoke(args, this.console);

	private object ConsoleOutput() => new
	{
		Out = this.console.Out.ToString(),
		Error = this.console.Error.ToString(),
	};
}
