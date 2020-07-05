using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;

namespace Responsible
{
	public static class ResponderExtensions
	{
		[Pure]
		public static ITestInstruction<TResult> ExpectWithin<TResult>(
			this ITestResponder<TResult> responder,
			int timeoutSeconds,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new ExpectTestResponse<TResult>(
				responder,
				TimeSpan.FromSeconds(timeoutSeconds),
				new SourceContext(sourceFilePath, sourceLineNumber));
	}
}