using System.Runtime.CompilerServices;

namespace Responsible.Bdd
{
	/// <summary>
	/// Keywords for building BDD-style tests for clearer source code and state strings.
	/// Helps in building tests with consistent style.
	/// </summary>
	// ReSharper disable once PartialTypeWithSinglePart, a Unity part exists
	public static partial class Keywords
	{
		/// <summary>
		/// Starts building a new BDD-style scenario.
		/// </summary>
		/// <param name="description">Description of the scenario</param>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T}"/>
		/// <returns>A scenario builder with the given description.</returns>
		public static ScenarioBuilder Scenario(
			string description,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			new ScenarioBuilder(description, memberName, sourceFilePath, sourceLineNumber);

		/// <summary>
		/// Creates a step that describes the initial context of a test.
		/// </summary>
		/// <inheritdoc cref="Docs.Inherit.BddKeyword"/>
		public static IBddStep Given(
			string description,
			ITestInstruction<object> instruction) =>
			new BddStep(instruction.GroupedAs($"Given {description}"));

		/// <summary>
		/// Creates a step that describes additional steps after a Given, When, or Then.
		/// </summary>
		/// <inheritdoc cref="Docs.Inherit.BddKeyword"/>
		public static IBddStep And(
			string description,
			ITestInstruction<object> instruction) =>
			new BddStep(instruction.GroupedAs($"And {description}"));

		/// <summary>
		/// Creates a step that describes additional steps after a Given, When, or Then.
		/// </summary>
		/// <inheritdoc cref="Docs.Inherit.BddKeyword"/>
		public static IBddStep But(
			string description,
			ITestInstruction<object> instruction) =>
			new BddStep(instruction.GroupedAs($"But {description}"));

		/// <summary>
		/// Creates a step that describes an event or action in a test.
		/// This can be a person interacting with the system,
		/// or it can be an event triggered by another system.
		/// </summary>
		/// <inheritdoc cref="Docs.Inherit.BddKeyword"/>
		public static IBddStep When(
			string description,
			ITestInstruction<object> instruction) =>
			new BddStep(instruction.GroupedAs($"When {description}"));

		/// <summary>
		/// Creates a step that describes an expected outcome of a test.
		/// </summary>
		/// <inheritdoc cref="Docs.Inherit.BddKeyword"/>
		public static IBddStep Then(
			string description,
			ITestInstruction<object> instruction) =>
			new BddStep(instruction.GroupedAs($"Then {description}"));

		/// <summary>
		/// A test instruction representing a BDD test step pending implementation.
		/// Will terminate the test as successful early, so that steps after it are skipped.
		/// Allows you to quickly write scenarios, and leave the implementation for later.
		/// </summary>
		public static readonly ITestInstruction<object> Pending = Responsibly.Do(
			PendingStepException.PendingMessage,
			() => throw new PendingStepException());
	}
}
