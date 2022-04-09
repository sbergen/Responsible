using System.Runtime.CompilerServices;

namespace Responsible.Bdd
{
	public static class Keywords
	{
		public static ScenarioBuilder Scenario(
			string description,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			new ScenarioBuilder(description, memberName, sourceFilePath, sourceLineNumber);

		public static ITestInstruction<T> Given<T>(
			string description,
			ITestInstruction<T> instruction) =>
			instruction.GroupedAs($"Given {description}");

		public static ITestInstruction<T> And<T>(
			string description,
			ITestInstruction<T> instruction) =>
			instruction.GroupedAs($"And {description}");

		public static ITestInstruction<T> When<T>(
			string description,
			ITestInstruction<T> instruction) =>
			instruction.GroupedAs($"When {description}");

		public static ITestInstruction<T> Then<T>(
			string description,
			ITestInstruction<T> instruction) =>
			instruction.GroupedAs($"Then {description}");
	}
}
