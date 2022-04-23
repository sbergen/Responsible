using System.IO.Abstractions;
using Gherkin;
using Gherkin.Ast;
using ResponsibleGherkin.Utilities;

namespace ResponsibleGherkin;

public record InputFile(
	Feature Feature,
	string ClassFileName,
	PartialConfiguration CommentsConfiguration,
	PartialConfiguration EditorConfigConfiguration)
{
	public static InputFile Read(IFileSystem fileSystem, string fileName)
	{
		using var fileReader = fileSystem.File.OpenText(fileName);
		var document = new Parser().Parse(fileReader);

		var outputFileName = $"{document.Feature.Name.ToPascalCase()}.cs";
		var editorConfig = EditorConfigHandler
			.ResolveEditorConfigProperties(outputFileName)
			.ConfigFromEditorConfigProperties();

		return new InputFile(
			document.Feature,
			outputFileName,
			CommentParser.Parse(document.Comments),
			editorConfig);
	}
}
