using System;

namespace Responsible.Utilities
{
	internal static class ActionExtensions
	{
		public static Func<object> ReturnUnit<T>(this Action<T> action, T arg) => () =>
		{
			action(arg);
			return Unit.Instance;
		};

		public static Func<object> ReturnUnit(this Action action) => () =>
		{
			action();
			return Unit.Instance;
		};
	}
}
