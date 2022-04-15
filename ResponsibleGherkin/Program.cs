// See https://aka.ms/new-console-template for more information

using Gherkin;
using ResponsibleGherkin;

var parser = new Parser();
var gherkinDocument = parser.Parse("/Users/sbergen/source/Responsible/ResponsibleGherkin/Test.feature");

var context = new GenerationContext("NS", "TestBase");

Console.Write(CodeGenerator.GenerateFile(
	gherkinDocument.Feature,
	CodeGenerator.FlavorType.NUnit,
	context));
