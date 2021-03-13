using System;

namespace Responsible
{
	/// <summary>
	/// Provides the required extension method on <see cref="ITestInstruction{T}"/>
	/// for it to be used with the LINQ query syntax.
	/// </summary>
	public static class LinqSupport
	{
		/// <summary>
		/// Standard extension method required to support LINQ query syntax.
		/// Should not be used directly.
		/// </summary>
		public static ITestInstruction<TResult> SelectMany<TSource, TSelector, TResult>(
			this ITestInstruction<TSource> source,
			Func<TSource, ITestInstruction<TSelector>> selector,
			Func<TSource, TSelector, TResult> resultSelector) =>
			source
				.ContinueWith(x => selector(x).Select(y => resultSelector(x, y)));
	}
}
