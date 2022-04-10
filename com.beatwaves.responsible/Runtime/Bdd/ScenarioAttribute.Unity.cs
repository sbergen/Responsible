using System;

namespace Responsible.Bdd
{
	/// <summary>
	/// Attribute for annotating a method as a BDD-style scenario.
	/// </summary>
	/// <seealso cref="FeatureAttribute"/>
	[JetBrains.Annotations.MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ScenarioAttribute : Attribute
	{
		public readonly string Description;

		/// <summary>
		/// Annotates a method as a BDD-style scenario returning steps, which will be run as a test case,
		/// if the containing class is annotated with <see cref="FeatureAttribute"/>.
		/// The return type of the method must be convertible to <c>IEnumerable&lt;IBddStep&gt;</c>.
		/// </summary>
		/// <example>
		/// <code>
		/// [Scenario("Example scenario")]
		/// public IBddStep[] Example() => new[]
		/// {
		///     Given("the setup is correct", ...),
		///     When("the user does something", ...),
		///     Then("the state should be updated correctly", ...),
		/// };
		/// </code>
		/// </example>
		/// <remarks>
		/// The class this method is in <b>must</b> use <see cref="FeatureAttribute"/>.
		/// </remarks>
		/// <param name="description">The description for the test scenario.</param>
		public ScenarioAttribute(string description)
		{
			this.Description = description;
		}
	}
}
