using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using UniRx;

// ReSharper disable ExplicitCallerInfoArgument

namespace Responsible
{
	// See Responsibly.WaitFor.cs for documentation
	public static partial class Responsibly
	{
		[Pure]
		public static ITestInstruction<T> Do<T>(
			Func<T> func,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<T>(
				func,
				new SourceContext(memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestInstruction<Unit> Do(
			Action action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<Unit>(
				() =>
				{
					action();
					return Unit.Default;
				},
				new SourceContext(memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static Func<TResult, ITestInstruction<Unit>> DoWithResult<TResult>(
			Action<TResult> action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			result => Do(() => action(result), memberName, sourceFilePath, sourceLineNumber);
	}
}