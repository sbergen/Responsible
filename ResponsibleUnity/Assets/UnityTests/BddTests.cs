using System;
using NUnit.Framework;
using Responsible.Bdd;
using static Responsible.Bdd.Keywords;
using static Responsible.Responsibly;

namespace Responsible.UnityTests
{
	[Category("BDD")]
	[Feature("BDD-style tests")]
	public class BddTests : BddTest
	{
		private static readonly ITestInstruction<object> NothingToDo = Do("Nothing to do here", () => { });

		private bool setUpCalled;
		private bool givenExecuted;
		private bool whenExecuted;
		private bool tearDownCalled;

		[SetUp]
		public void SetUp()
		{
			this.setUpCalled = true;
			this.givenExecuted = false;
			this.whenExecuted = false;
		}

		[TearDown]
		public void TearDown()
		{
			this.tearDownCalled = true;
		}

		// It's really hard to know for sure if TearDown gets run with custom attributes,
		// However, if SetUp is called, we can be pretty sure TearDown is also called.
		// But lets do this just for the heck of it anyway.
		// If anyone happens to look at this code and has ideas, please hit me up!
		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Assert.IsTrue(this.tearDownCalled, "Tear down should have been called at least once");
		}

		[Scenario("A basic BDD-style test runs without error")]
		public IBddStep[] BasicTest() => new[]
		{
			Given(
				"the test is set up properly",
				Do("Execute Given", () => this.givenExecuted = true)),
			When(
				"we execute the when step",
				Do("Execute When", () => this.whenExecuted = true)),
			Then(
				"the state of the test class should be in the final expected state",
				Do(
					"Assert state",
					() => Assert.AreEqual(
						(true, true, true),
						(this.setUpCalled, this.givenExecuted, this.whenExecuted)))),
		};

		[Scenario("A test with pending steps should be terminated early without error")]
		public IBddStep[] PendingStepsTest() => new[]
		{
			Given("the test has a pending step in the beginning", Pending),
			Then(
				"an error at later steps will not fail the test",
				Do("Throw exception", () => throw new Exception())),
		};

		[Ignore("Should be skipped!")]
		[Scenario("A test with an IgnoreAttribute should be ignored")]
		public IBddStep[] IgnoreAttributeShouldBeApplied() => new[]
		{
			Given("the test has an IgnoreAttribute", NothingToDo),
			And("it throws and exception", Do("Throw exception", () => throw new Exception())),
			Then("the test should not be executed", Pending),
		};

		[Scenario("A scenario with one parameter should pass it to the test method", "expected")]
		public IBddStep[] ParametersShouldBePassed(string parameter) => new[]
		{
			Given("a scenario with a parameter in the attribute", NothingToDo),
			Then(
				"the parameter passed in should match the expected value",
				Do("Check the parameter", () => Assert.AreEqual("expected", parameter))),
		};

		[Scenario("A scenario with two parameters should pass them to the test method", "expected", 42)]
		public IBddStep[] ParametersShouldBePassed(string param1, int param2) => new[]
		{
			Given("a scenario with a parameter in the attribute", NothingToDo),
			Then(
				"the parameter passed in should match the expected value",
				Do(
					"Check the parameter",
					() => Assert.AreEqual(("expected", 42), (param1, param2)))),
		};

		// Testing this is a bit tricky, and requires manual inspection for now :(
		// Would be great to find some other way to do this...
		[Scenario("There should be two of us, I log 'First!'", "First!")]
		[Scenario("There should be two of us, I log 'Second!'", "Second!")]
		public IBddStep[] MultipleScenariosShouldBeAllowed(string thingToLog) => new[]
		{
			Given("a test with two scneario attributes", NothingToDo),
			Then(
				"the correct thing should be logged (take a look for yourself)",
				Do("Log a message", () => UnityEngine.Debug.Log(thingToLog))),
		};

		[Scenario("Attributes in feature classes should be taken into account")]
		public IBddStep[] AttributesShouldBeInheritable() => new[]
		{
			Given("a scenario within a feature-class with the category BDD", NothingToDo),
			Then(
				"the parent of the test should have the BDD category",
				Do(
					"Assert test category",
					() => CollectionAssert.Contains(
						TestContext.CurrentTestExecutionContext.CurrentTest.Parent.Properties["Category"],
						"BDD"))),
		};
	}
}
