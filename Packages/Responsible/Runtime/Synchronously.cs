using System;
using System.Runtime.CompilerServices;
using Responsible.Context;
using Responsible.TestInstructions;
using UniRx;

// ReSharper disable ExplicitCallerInfoArgument

namespace Responsible
{
	public static class Synchronously
	{
		public static ITestInstruction<T> Do<T>(
			Func<T> action,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<T>(action, new SourceContext(sourceFilePath, sourceLineNumber));

		public static ITestInstruction<Unit> Do(
			Action action,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<Unit>(
				() =>
				{
					action();
					return Unit.Default;
				},
				new SourceContext(sourceFilePath, sourceLineNumber));

		public static Func<TResult, ITestInstruction<Unit>> DoWithResult<TResult>(
			Action<TResult> action,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			result => Do(() => action(result), sourceFilePath, sourceLineNumber);
	}
}