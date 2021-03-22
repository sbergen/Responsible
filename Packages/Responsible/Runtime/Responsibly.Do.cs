using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using Responsible.Utilities;

namespace Responsible
{
	// See Responsibly.WaitFor.cs for documentation
	public static partial class Responsibly
	{
		/// <summary>Execute a synchronous action safely, and return a result.
		/// </summary>
		/// <param name="func">Function to call when the test instruction is executed.</param>
		/// <returns>A test instruction which calls <paramref name="func"/> and returns its value when executed.</returns>
		/// <remarks>
		/// This is not an overload of <see cref="Do"/>, because C# is bad at overload resolution with lambdas.
		/// </remarks>
		/// <typeparam name="T">Result type of the returned test instruction and <paramref name="func"/></typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithDescription{T}"/>
		[Pure]
		public static ITestInstruction<T> DoAndReturn<T>(
			string description,
			Func<T> func,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<T>(
				description,
				func,
				new SourceContext(nameof(DoAndReturn), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>Execute a synchronous action safely, and return <see cref="Unit.Default"/>.</summary>
		/// <param name="action">Action to call when the test instruction is executed.</param>
		/// <returns>
		/// A test instruction which calls <paramref name="action"/> and returns true when executed.
		/// </returns>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithDescription{T}"/>
		[Pure]
		public static ITestInstruction<object> Do(
			string description,
			Action action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<object>(
				description,
				action.ReturnUnit(),
				new SourceContext(nameof(Do), memberName, sourceFilePath, sourceLineNumber));
	}
}
