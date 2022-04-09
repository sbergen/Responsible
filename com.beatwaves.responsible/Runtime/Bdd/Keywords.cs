using System.Runtime.CompilerServices;

namespace Responsible.Bdd
{
	/// <summary>
	/// Keywords for building BDD-style tests for clearer source code and state strings.
	/// Helps in building tests with consistent style.
	/// </summary>
	public static class Keywords
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
		/// <inheritdoc cref="Docs.Inherit.BddKeyword{T}"/>
		public static IBddStep<T> Given<T>(
			string description,
			ITestInstruction<T> instruction) =>
			new BddStep<T>(instruction.GroupedAs($"Given {description}"));

		/// <summary>
		/// Creates a step that describes additional initial context of a test.
		/// </summary>
		/// <inheritdoc cref="Docs.Inherit.BddKeyword{T}"/>
		public static IBddStep<T> And<T>(
			string description,
			ITestInstruction<T> instruction) =>
			new BddStep<T>(instruction.GroupedAs($"And {description}"));

		/// <summary>
		/// Creates a step that describes an event or action in a test.
		/// </summary>
		/// <inheritdoc cref="Docs.Inherit.BddKeyword{T}"/>
		public static IBddStep<T> When<T>(
			string description,
			ITestInstruction<T> instruction) =>
			new BddStep<T>(instruction.GroupedAs($"When {description}"));

		/// <summary>
		/// Creates a step that describes an expected outcome of a test.
		/// </summary>
		/// <inheritdoc cref="Docs.Inherit.BddKeyword{T}"/>
		public static IBddStep<T> Then<T>(
			string description,
			ITestInstruction<T> instruction) =>
			new BddStep<T>(instruction.GroupedAs($"Then {description}"));
	}
}
