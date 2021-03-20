using System;

namespace Responsible.NoRx.Utilities
{
	internal static class ActionExtensions
	{
		public static Func<bool> ReturnTrue<T>(this Action<T> action, T arg) => () =>
		{
			action(arg);
			return true;
		};

		public static Func<bool> ReturnTrue(this Action action) => () =>
		{
			action();
			return true;
		};
	}
}
