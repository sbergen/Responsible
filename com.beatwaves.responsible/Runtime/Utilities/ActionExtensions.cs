using System;
using JetBrains.Annotations;
using Responsible.State;

namespace Responsible.Utilities
{
	internal static class ActionExtensions
	{
		[CanBeNull]
		public static Action<StateStringBuilder, TState> DiscardingState<TState>(
			[CanBeNull] this Action<StateStringBuilder> extraContext) => extraContext != null
			? (builder, _) => extraContext(builder)
			: null;

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
