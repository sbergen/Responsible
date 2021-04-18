using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;

namespace Responsible
{
	// See Responsibly.WaitFor.cs for documentation
	public static partial class Responsibly
	{
		/// <summary>Constructs a test instruction that returns a value synchronously.</summary>
		/// <returns>A test instruction returning <paramref name="value"/>.</returns>
		/// <param name="value">Value to return from the test instruction.</param>
		/// <typeparam name="T">Result type of the returned test instruction.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T}"/>
		[Pure]
		public static ITestInstruction<T> Return<T>(
			T value,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<T>(
				$"Return '{value}'",
				() => value,
				new SourceContext(nameof(Return), memberName, sourceFilePath, sourceLineNumber));
	}
}
