using System.Linq;
using NUnit.Framework;

namespace ResponsibleGherkin.Tests;

public class UsingDirectivesTests
{
	[Test]
	public void UsingDirectives_AreSortedAlphabetically()
	{
		var lines = UsingDirectivesGenerator.Generate(
			Flavor.NUnit with { RequiredNamespaces = new[] { "aac", "aaa", "aab" } });

		Assert.AreEqual(
			new[]
			{
				"using aaa;",
				"using aab;",
				"using aac;",
			},
			lines.Select(line => line.Content).Take(3).ToArray());
	}

}
