using System;

namespace Responsible.Bdd
{
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
	/// <seealso cref="FeatureAttribute"/>
	[JetBrains.Annotations.MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class ScenarioAttribute : Attribute
	{
		internal readonly string Description;
		internal readonly object[] Parameters;

		/// <summary>
		/// Annotates a method as a BDD-style scenario returning steps, which will be run as a test case,
		/// if the containing class is annotated with <see cref="FeatureAttribute"/>.
		/// The return type of the method must be convertible to <c>IEnumerable&lt;IBddStep&gt;</c>.
		/// </summary>
		/// <param name="description">The description for the test scenario.</param>
		/// <param name="parameters">The parameters to pass to the test method.</param>
		public ScenarioAttribute(string description, params object[] parameters)
		{
			this.Description = description;
			this.Parameters = parameters ?? Array.Empty<object>();
		}
	}
}
