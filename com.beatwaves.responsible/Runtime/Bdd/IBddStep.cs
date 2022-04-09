namespace Responsible.Bdd
{
	/// <summary>
	/// Tag interface for BDD-style test instructions.
	/// This exists only to be able to enforce that all instructions used in a scenario
	/// are properly wrapped in other BDD-keywords.
	/// Functionally no different from <see cref="ITestInstruction{T}"/>.
	/// </summary>
	/// <typeparam name="T">Result type of the test instruction.</typeparam>
	public interface IBddStep<out T> : ITestInstruction<T>
	{
	}
}
