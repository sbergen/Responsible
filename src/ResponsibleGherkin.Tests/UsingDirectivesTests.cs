using System.Linq;
using FluentAssertions;
using ResponsibleGherkin.Generators;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class UsingDirectivesTests
{
	[Fact]
	public void UsingDirectives_AreSortedAlphabetically()
	{
		var lines = UsingDirectivesGenerator.Generate(
			Flavor.NUnit with { RequiredNamespaces = new[] { "aac", "aaa", "aab" } });

		var generatedUsingDirectives = lines.Select(line => line.Content).ToArray();

		generatedUsingDirectives.Should().ContainInOrder("using aaa;", "using aab;", "using aac;");
	}
}
