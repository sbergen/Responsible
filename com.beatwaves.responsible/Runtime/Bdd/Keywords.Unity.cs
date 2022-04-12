using NUnit.Framework;

namespace Responsible.Bdd
{
	// Unity-specific BDD keywords.
	// Some are just NUnit-specific, as we don't depend on it in the vanilla C# parts.
	public static partial class Keywords
	{
		private const string PendingMessage = "BDD step implementation pending";

		/// <summary>
		/// A test instruction representing a BDD test step pending implementation.
		/// Will terminate the test as ignored, so that steps after it are skipped.
		/// Allows you to quickly write scenarios, and leave the implementation for later.
		/// </summary>
		public static readonly ITestInstruction<object> Pending = Responsibly.Do(
			PendingMessage, () => Assert.Ignore(PendingMessage));
	}
}
