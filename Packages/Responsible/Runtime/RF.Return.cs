using System;
using JetBrains.Annotations;
using Responsible.TestInstructions;

namespace Responsible
{
	// See RF.WaitFor.cs for documentation
	public static partial class RF
	{
		/// <summary>
		/// Constructs a test instruction that return a value synchronously.
		/// Wouldn't be a monad without this ;)
		/// </summary>
		[Pure]
		public static ITestInstruction<T> Return<T>(T value) =>
			new FuncTestInstruction<T>(() => value);

		/// <summary>
		/// Constructs a test instruction that return a value deferredly.
		/// Useful when sequencing things.
		/// </summary>
		[Pure]
		public static ITestInstruction<T> ReturnDeferred<T>(Func<T> create) =>
			new FuncTestInstruction<T>(create);
	}
}