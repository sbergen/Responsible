using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;
using Responsible.NoRx.TestWaitConditions;

namespace Responsible.NoRx
{
	/// <summary>
	/// Main class for constructing primitive wait conditions and instructions,
	/// and composing responders.
	/// </summary>
	/// <remarks>
	/// Instead of using a class like <c>WaitFor</c>, we try to avoid conflicting class names by having this class.
	/// It also allows you to use either <c>Responsibly.WaitForCondition</c> or
	/// <c>using static Responsible.Responsibly;</c> and simply <c>WaitForCondition</c>.
	/// </remarks>
	public static partial class Responsibly
	{
		[Pure]
		public static ITestWaitCondition<T> WaitForConditionOn<T>(
			string description,
			Func<T> getObject,
			Func<T, bool> condition,
			Action<StateStringBuilder> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<T>(
				description,
				getObject,
				condition,
				extraContext,
				new SourceContext(nameof(WaitForConditionOn), memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestWaitCondition<bool> WaitForCondition(
			string description,
			Func<bool> condition,
			Action<StateStringBuilder> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<bool>(
				description,
				() => true,
				_ => condition(),
				extraContext,
				new SourceContext(nameof(WaitForCondition), memberName, sourceFilePath, sourceLineNumber));
	}
}
