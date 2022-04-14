---
uid: Responsible.Bdd
summary: *content
---
**Experimental:** Features in this namespace might still change.
If you like what you see, please leave feedback in
[discussion on GitHub](https://github.com/sbergen/Responsible/discussions)!

Contains utilities to build BDD-style tests,
using Gherkin-style keywords (Given, When, Then, etc).
Essentially forms an embedded DSL on top of the core of Responsible
to describe the top-level of tests.

The following is an example test using NUnit:

```cs
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible;
using static Responsible.Bdd.Keywords;

public class ExampleBddTest
{
	private TestInstructionExecutor executor;

	// TODO: set up and tear down the executor properly

	[Test]
	public Task Example() => this.executor.RunScenario(
		Scenario("Example scenario"),
		Given("the setup is correct", Pending),
		When("the user does something", Pending),
		Then("the state should be updated correctly", Pending));
}
```

> **On Unity Test Runner 1.X**, you can use
> `UnityTest`, `IEnumerator` and `YieldScenario` respectively.

The `Pending`-keyword will terminate the test early,
and can be used to write scenarios before implementation.
As the implementation of the feature progresses,
pending steps can be replaced with actual test instructions.
