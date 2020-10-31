using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using Responsible.Utilities;
using UniRx;

namespace Responsible
{
	// See Responsibly.WaitFor.cs for documentation
	public static partial class Responsibly
	{
		/// <summary>
		/// Do an action safely, and return a result.
		/// </summary>
		/// <remarks>
		/// This is not an overload of Do, because C# is bad at overload resolution with lambdas.
		/// </remarks>
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

		/// <summary>
		/// Do an action safely, and don't return any result.
		/// </summary>
		[Pure]
		public static ITestInstruction<Unit> Do(
			string description,
			Action action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<Unit>(
				description,
				action.AsUnitFunc(),
				new SourceContext(nameof(DoAndReturn), memberName, sourceFilePath, sourceLineNumber));
	}
}