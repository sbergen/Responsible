using System.Threading.Tasks;
using Responsible.Bdd;

namespace Responsible
{
	/// <summary>
	/// Extensions that make it easier and or prettier to run BDD-style tests.
	/// </summary>
	public static class BddExtensions
	{
		/// <summary>
		///	Helper method to concisely define BDD-style scenarios as a single
		/// expression-bodied test method.
		/// </summary>
		/// <param name="executor">Test executor to use to run the scenario</param>
		/// <param name="scenario">Scenario definition (provides name and source context)</param>
		/// <param name="steps">Steps the scenario consists of.</param>
		/// <returns>Task representing the run state of the scenario.</returns>
		public static Task RunScenario(
			this TestInstructionExecutor executor,
			ScenarioBuilder scenario,
			params IBddStep[] steps) =>
			scenario
				.WithSteps(steps)
				.ToTask(executor);

#if UNITY_2019_1_OR_NEWER // Minimum supported version

		/// <inheritdoc cref="RunScenario"/>
		/// <returns>Yield instruction that runs the scenario.</returns>
		public static System.Collections.IEnumerator YieldScenario(
			this TestInstructionExecutor executor,
			ScenarioBuilder scenario,
			params IBddStep[] steps) =>
			scenario
				.WithSteps(steps)
				.ToYieldInstruction(executor);
#endif
	}
}
