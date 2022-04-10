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
		/// Annotates a method as a BDD-style scenario, which will be run as a test case.
		/// </summary>
		/// <remarks>
		/// The class this method is in must use <see cref="FeatureAttribute"/>.
		/// </remarks>
		/// <param name="description">The description for the test scenario.</param>
		public ScenarioAttribute(string description)
		{
			this.Description = description;
		}
	}
}
