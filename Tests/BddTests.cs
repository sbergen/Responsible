using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;
using static Responsible.Bdd.Keywords;

namespace Responsible.Tests
{
	public class BddTests : ResponsibleTestBase
	{
		private bool givenExecuted;
		private bool andExecuted;
		private bool whenExecuted;
		private bool thenExecuted;
		private ITestInstruction<object> scenario;

		[SetUp]
		public void SetUp()
		{
			this.givenExecuted = false;
			this.andExecuted = false;
			this.whenExecuted = false;
			this.thenExecuted = false;

			this.scenario = Scenario(
				"Test scenario",
				Given(
					"given",
					Do("given inner", () => this.givenExecuted = true)),
				And(
					"and",
					Do("and inner", () => this.andExecuted = true)),
				When(
					"when",
					Do("when inner", () => this.whenExecuted = true)),
				Then(
					"then",
					Do("then inner", () => this.thenExecuted = true)));
		}

		[Test]
		public void Scenario_ExecutesAllSteps()
		{
			this.scenario.ToTask(this.Executor);
			Assert.AreEqual(
				(true, true, true, true),
				(this.givenExecuted, this.andExecuted, this.whenExecuted, this.thenExecuted));
		}

		[Test]
		public void StateString_ContainsExpectedContent()
		{
			var stateString = this.scenario.CreateState().ToString();

			StateAssert.StringContainsInOrder(stateString)
				.NotStarted("Scenario: Test scenario")
				.Details("  ").NotStarted("Given given")
				.Details("    ").NotStarted("given inner")
				.Details("  ").NotStarted("And and")
				.Details("    ").NotStarted("and inner")
				.Details("  ").NotStarted("When when")
				.Details("    ").NotStarted("when inner")
				.Details("  ").NotStarted("Then then")
				.Details("    ").NotStarted("then inner");
		}
	}
}
