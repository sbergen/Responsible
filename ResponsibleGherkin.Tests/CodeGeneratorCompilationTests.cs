using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using static ResponsibleGherkin.Tests.TestFeatures;

namespace ResponsibleGherkin.Tests;

public class CodeGeneratorCompilationTests
{
	private static readonly Type[] MetaDataSourceTypes =
	{
		typeof(TestAttribute),
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

namespace {DefaultContext.Namespace}
{{
	public abstract class {DefaultContext.BaseClass}
	{{
		protected TestInstructionExecutor {DefaultContext.ExecutorName} {{ get; private set; }}
	}}
}}
";

	[Test]
	public void Precondition_TestBaseClass_CompilesWithoutDiagnostics() =>
		AssertCodeCompilesWithoutDiagnostics(TestBaseClassCode);

	[TestCase("BasicFeature")]
	public void Feature_CompilesWithoutDiagnostics_WithNUnit(string featureName)
	{
		var document = LoadFeature(featureName);
		var code = CodeGenerator.GenerateFile(document.Feature, CodeGenerator.FlavorType.NUnit, DefaultContext);
		AssertCodeCompilesWithoutDiagnostics(TestBaseClassCode, code);
	}

	private static void AssertCodeCompilesWithoutDiagnostics(params string[] codes)
	{
		var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
		var syntaxTrees = codes.Select(text => CSharpSyntaxTree.ParseText(text));

		var compilation = CSharpCompilation.Create("MyTests", syntaxTrees, References, options);
		var diagnostics = compilation.GetDiagnostics();

		Assert.IsEmpty(diagnostics);
	}
}
