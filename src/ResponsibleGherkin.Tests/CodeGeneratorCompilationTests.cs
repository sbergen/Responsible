using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using static ResponsibleGherkin.Tests.TestData;

namespace ResponsibleGherkin.Tests;

public class CodeGeneratorCompilationTests
{
	private static readonly Type[] MetaDataSourceTypes =
	{
		typeof(FactAttribute),
		typeof(Responsible.Responsibly),
	};

	private static readonly string[] MetaDataAssemblyNames =
	{
		"netstandard, Version=2.0.0.0",
		"System.Runtime, Version=6.0.0.0",
		"System.Private.CoreLib, Version=6.0.0.0",
	};

	private static readonly PortableExecutableReference[] References = Enumerable
		.Concat(
			MetaDataSourceTypes.Select(type => type.Assembly.Location),
			MetaDataAssemblyNames.Select(name => Assembly.Load(name).Location))
		.Select(file => MetadataReference.CreateFromFile(file))
		.ToArray();

	// Minimal code required to compile, will not run properly
	private static readonly string TestBaseClassCode = $@"
using Responsible;

namespace {DefaultConfiguration.Namespace}
{{
	public abstract class {DefaultConfiguration.BaseClass}
	{{
		protected TestInstructionExecutor {DefaultConfiguration.ExecutorName} {{ get; private set; }}
	}}
}}
";

	[Fact]
	public void Precondition_TestBaseClass_CompilesWithoutDiagnostics() =>
		AssertCodeCompilesWithoutDiagnostics(TestBaseClassCode);

	[Theory]
	[InlineData(MinimalFeature)]
	[InlineData(BasicFeature)]
	public void Feature_CompilesWithoutDiagnostics_WithXUnit(string featureName)
	{
		var document = LoadFeature(featureName);
		var code = CodeGenerator.GenerateClass(document.Feature, DefaultConfiguration);
		AssertCodeCompilesWithoutDiagnostics(TestBaseClassCode, code.BuildFileContent());
	}

	private static void AssertCodeCompilesWithoutDiagnostics(params string[] codes)
	{
		var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
		var syntaxTrees = codes.Select(text => CSharpSyntaxTree.ParseText(text));

		var compilation = CSharpCompilation.Create("MyTests", syntaxTrees, References, options);
		var diagnostics = compilation.GetDiagnostics();

		diagnostics.Should().BeEmpty();
	}
}
