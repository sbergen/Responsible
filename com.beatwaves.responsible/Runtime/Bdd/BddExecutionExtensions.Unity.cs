using System.Collections;
using static Responsible.Bdd.Keywords;

namespace Responsible.Bdd
{
	public static class BddExecutionExtensions
	{
		public static IEnumerator RunScenario(
			this TestInstructionExecutor executor,
			string description,
			params IBddStep[] steps) =>
			Scenario(description)
				.WithSteps(steps)
				.ToYieldInstruction(executor);
	}
}
