using System.Linq;
using Xunit;

namespace ResponsibleGherkin.Tests;

public class UsingDirectivesTests
{
	[Fact]
	public void UsingDirectives_AreSortedAlphabetically()
	{
		var lines = UsingDirectivesGenerator.Generate(
			Flavor.NUnit with { RequiredNamespaces = new[] { "aac", "aaa", "aab" } });

		Assert.Equal(
			new[]
			{
				"using aaa;",
				"using aab;",
				"using aac;",
			},
			lines.Select(line => line.Content).Take(3).ToArray());
	}
}
