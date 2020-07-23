using System;

namespace Responsible
{
	public static class LinqSupport
	{
		public static ITestInstruction<TResult> SelectMany<TSource, TSelector, TResult>(
			this ITestInstruction<TSource> source,
			Func<TSource, ITestInstruction<TSelector>> selector,
			Func<TSource, TSelector, TResult> resultSelector) =>
			source
				.ContinueWith(x => selector(x).Select(y => resultSelector(x, y)));
	}
}