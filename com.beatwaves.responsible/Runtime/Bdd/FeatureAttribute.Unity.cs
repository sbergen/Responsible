using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Responsible.Bdd
{
	/// <summary>
	/// Attribute for annotating a class as BDD-style tests for a feature.
	/// </summary>
	/// <seealso cref="ScenarioAttribute"/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class FeatureAttribute : Attribute, IApplyToTest
	{
		private readonly string description;

		/// <summary>
		/// Annotates a class as a feature test suite with the given description.
		/// </summary>
		/// <param name="description">Description of the feature</param>
		public FeatureAttribute(string description) => this.description = $"Feature: {description}";

		void IApplyToTest.ApplyToTest(Test test) => test.Name = this.description;
	}
}
