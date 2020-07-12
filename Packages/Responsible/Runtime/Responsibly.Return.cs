using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;

namespace Responsible
{
	// See Responsibly.WaitFor.cs for documentation
	public static partial class Responsibly
	{
		/// <summary>
		/// Constructs a test instruction that return a value synchronously.
		/// Wouldn't be a monad without this ;)
		/// </summary>
		/// <summary>
		/// Constructs a test instruction that return a value synchronously.
		/// Wouldn't be a monad without this ;)
		/// </summary>
		[Pure]
		public static ITestInstruction<T> Return<T>(
			T value,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<T>(
				"Return '{value}'",
				() => value,
				new SourceContext(nameof(Return), memberName, sourceFilePath, sourceLineNumber));
	}
}