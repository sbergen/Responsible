using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.NoRx.Context;
using Responsible.NoRx.TestInstructions;

namespace Responsible.NoRx
{
	/// <summary>
	/// Contains extension methods on <see cref="ITestWaitCondition{T}"/>.
	/// </summary>
	public static class TestWaitCondition
	{
		[Pure]
		public static ITestInstruction<T> ExpectWithinSeconds<T>(
			this ITestWaitCondition<T> condition,
			double timeout,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new ExpectConditionInstruction<T>(
				condition,
				TimeSpan.FromSeconds(timeout),
				new SourceContext(nameof(ExpectWithinSeconds), memberName, sourceFilePath, sourceLineNumber));
	}
}
