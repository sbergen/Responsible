using System;

namespace Responsible.NoRx.Utilities
{
	internal static class ActionExtensions
	{
		public static Func<Nothing> ReturnNothing<T>(this Action<T> action, T arg) => () =>
		{
			action(arg);
			return default;
		};

		public static Func<Nothing> ReturnNothing(this Action action) => () =>
		{
			action();
			return default;
		};
	}
}
