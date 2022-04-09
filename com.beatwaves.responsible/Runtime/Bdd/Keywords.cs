namespace Responsible.Bdd
{
	public static class Keywords
	{
		// TODO: Source context?

		public static ITestInstruction<object> Scenario(
			string description,
			params ITestInstruction<object>[] steps) =>
			steps.Sequence().GroupedAs($"Scenario: {description}");

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
