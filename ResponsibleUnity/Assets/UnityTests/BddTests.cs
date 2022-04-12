using System;
using NUnit.Framework;
using Responsible.Bdd;
using static Responsible.Bdd.Keywords;
using static Responsible.Responsibly;

namespace Responsible.UnityTests
{
	[Feature("BDD-style tests")]
	public class BddTests : BddTest
	{
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

		[Scenario("A test with pending steps should be skipped")]
		public IBddStep[] PendingStepsTest() => new[]
		{
			Given("The test has a pending step in the beginning", Pending),
			Then(
				"An error at later steps will not fail the test",
				Do("Throw exception", () => throw new Exception())),
		};

		[Ignore("Should be skipped!")]
		[Scenario("A test with an IgnoreAttribute should be ignored")]
		public IBddStep[] IgnoreAttributeShouldBeApplied() => new[]
		{
			Given("The test has an IgnoreAttribute and throws and exception",
				Do("Throw exception", () => throw new Exception())),
			Then("The test should not be executed", Pending),
		};
	}
}
