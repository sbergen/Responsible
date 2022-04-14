using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Responsible.Bdd;
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

			this.scenario = Scenario("Test scenario").WithSteps(
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

		[Test]
		public void Pending_CancelsTask()
		{
			var task = Pending.ToTask(this.Executor);
			Assert.IsTrue(task.IsCanceled);
		}

		// The keywords below are considered omissible from the instruction stacks, they only show up in the
		// operation states.
		[TestCase("Given")]
		[TestCase("And")]
		[TestCase("When")]
		[TestCase("Then")]
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void InstructionStack_DoesNotContainKeyword(string methodName)
		{
			var throwInstruction = Do("Throw", () => throw new Exception());
			var method = typeof(Keywords)
				.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
			var instruction = (ITestInstruction<object>)method.Invoke(
				null,
				new object[] { methodName, throwInstruction });
			var state = instruction.CreateState();

			state.ToTask(this.Executor);

			StringAssert.DoesNotContain($"[{methodName}]", state.ToString());
		}

		// Partially for implementation detail reasons, and partially because it's nice to at least see
		// the top-level scenario in operation stacks, make sure the scenario location is properly recorded.
		[Test]
		public void InstructionStack_DoesContainScenarioKeywordAndLocation()
		{
			var failingScenario = Scenario("Failing scenario").WithSteps(
				Given("nothing", Do("NOP", () => { })),
				Then("throw", Do("Throw", () => throw new Exception("Test exception"))));
			var state = failingScenario.CreateState();

			state.ToTask(this.Executor);

			StringAssert.Contains(
				$"[{nameof(Scenario)}] {GetCallerDetails()}",
				state.ToString());
		}

		private static string GetCallerDetails(
			[CallerMemberName] string member = null,
			[CallerFilePath] string file = null) =>
			$"{member} (at {file}";
	}
}
