using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Responsible.Bdd
{
	/// <summary>
	/// Helper class to build scenarios with proper source context.
	/// </summary>
	public sealed class ScenarioBuilder
	{
		private readonly string description;
		private readonly string memberName;
		private readonly string sourceFilePath;
		private readonly int sourceLineNumber;

		internal ScenarioBuilder(string description, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			this.description = description;
			this.memberName = memberName;
			this.sourceFilePath = sourceFilePath;
			this.sourceLineNumber = sourceLineNumber;
		}


		/// <summary>
		/// Builds a test instruction for the scenario from the given steps.
		/// </summary>
		/// <param name="steps">Steps to execute as part of the scenario.</param>
		/// <returns>A test instruction which will execute the scenario.</returns>
		[Pure]
		[SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
		public ITestInstruction<object> WithSteps(params IBddStep<object>[] steps) => steps
			.Sequence(this.memberName, this.sourceFilePath, this.sourceLineNumber, nameof(Keywords.Scenario))
			.GroupedAs($"Scenario: {this.description}");
	}
}
