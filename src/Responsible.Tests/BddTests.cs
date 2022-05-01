using System;
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
		private bool butExecuted;
		private ITestInstruction<object> scenario = null!;

		[SetUp]
		public void SetUp()
		{
			this.givenExecuted = false;
			this.andExecuted = false;
			this.whenExecuted = false;
			this.thenExecuted = false;
			this.butExecuted = false;

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
					Do("then inner", () => this.thenExecuted = true)),
				But(
					"but",
					Do("but inner", () => this.butExecuted = true)));
		}

		[Test]
		public void Scenario_ExecutesAllSteps()
		{
			this.scenario.ToTask(this.Executor);
			Assert.AreEqual(
				(true, true, true, true, true),
				(this.givenExecuted, this.andExecuted, this.whenExecuted, this.thenExecuted, this.butExecuted));
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
				.Details("    ").NotStarted("then inner")
				.Details("  ").NotStarted("But but")
				.Details("    ").NotStarted("but inner");
		}

		[Test]
		public void Pending_CancelsTask()
		{
			var didContinue = false;
			var task = Pending
				.ContinueWith(Do("Continue", () => didContinue = true))
				.ToTask(this.Executor);

			Assert.IsFalse(didContinue, "Pending should terminate test early");
			Assert.IsTrue(task.IsCanceled, "Task should be canceled after Pending is executed");
		}

		[Test]
		public void RunScenario_ThrowsInvalidOperationException_IfEmpty()
		{
			Assert.Throws<InvalidOperationException>(() =>
				this.Executor.RunScenario(Scenario("Test scenario")));
		}

		[Test]
		public void RunScenario_ExecutesAllSteps()
		{
			this.Executor.RunScenario(
				Scenario("Test scenario"),
				Given("G", Do("G", () => this.givenExecuted = true)),
				When("W", Do("W", () => this.whenExecuted = true)));

			Assert.AreEqual((true, true), (this.givenExecuted, this.whenExecuted));
		}

		// The keywords below are considered omissible from the instruction stacks, they only show up in the
		// operation states.
		[TestCase("Given")]
		[TestCase("And")]
		[TestCase("When")]
		[TestCase("Then")]
		[TestCase("But")]
		public void InstructionStack_DoesNotContainKeyword(string methodName)
		{
			var throwInstruction = Do("Throw", () => throw new Exception());
			var method = typeof(Keywords)
				.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
			var instruction = (ITestInstruction<object>)method!.Invoke(
				null,
				new object[] { methodName, throwInstruction })!;
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
			[CallerMemberName] string? member = null,
			[CallerFilePath] string? file = null) =>
			$"{member} (at {file}";
	}
}
