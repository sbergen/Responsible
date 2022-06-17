using System;
using NUnit.Framework;
using Responsible.State;

namespace Responsible.Tests
{
	// Cover the public interface, everything else is (should be) tested
	// through other tests.
	public class StateStringBuilderTests
	{
		private static readonly string NewLine = Environment.NewLine;

		[Test]
		public void AddNestedDetails_ProducesExpectedValue_WhenDetailsAreMultiline()
		{
			var builder = new StateStringBuilder();
			builder.AddNestedDetails("Primary", b => b.AddDetails("Nested1\nNested2"));
			Assert.AreEqual(
				$"Primary{NewLine}  Nested1{NewLine}  Nested2",
				builder.ToString());
		}

		[Test]
		public void Indentation_WorksAsExpected_InComplexScenario()
		{
			var builder = new StateStringBuilder();
			builder.AddNestedDetails(
				"One",
				b =>
				{
					b.AddNestedDetails("Two", _ => b.AddDetails("Three"));
					b.AddDetails("Four");
				});
			builder.AddDetails("Five");

			Assert.AreEqual(
				$"One{NewLine}  Two{NewLine}    Three{NewLine}  Four{NewLine}Five",
				builder.ToString());
		}
	}
}
