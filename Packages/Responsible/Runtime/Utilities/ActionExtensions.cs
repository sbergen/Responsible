using System;
using UniRx;

namespace Responsible.Utilities
{
	internal static class ActionExtensions
	{
		public static Func<Unit> AsUnitFunc<T>(this Action<T> action, T arg) => () =>
		{
			action(arg);
			return Unit.Default;
		};

		public static Func<Unit> AsUnitFunc(this Action action) => () =>
		{
			action();
			return Unit.Default;
		};
	}
}