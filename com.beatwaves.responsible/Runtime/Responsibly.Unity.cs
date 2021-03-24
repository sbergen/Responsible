using System;
using System.Collections;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using Responsible.Context;
using Responsible.Unity;

namespace Responsible
{
	public static partial class Responsibly
	{
		/// <summary>
		/// **Unity-only!**
		///
		/// Constructs a wait condition, which will call <paramref name="getObject"/> on every frame,
		/// and check <paramref name="constraint"/> on the returned object.
		/// Will complete once the constraint is fulfilled,
		/// returning the last value returned by <paramref name="getObject"/>.
		/// When constructing the state description, will add the constraint state to the description.
		/// </summary>
		/// /// <returns>
		/// A wait condition, which completes with the value last returned from <paramref name="getObject"/>,
		/// when <paramref name="constraint"/> is met for it.
		/// </returns>
		/// <param name="objectDescription">
		/// Description of the object to be tested with <paramref name="constraint"/>,
		/// to be included in the operation state description.
		/// </param>
		/// <param name="getObject">Function that returns the object to test <paramref name="constraint"/> on.</param>
		/// <param name="constraint">Constraint to check with the return value of <paramref name="getObject"/>.</param>
		/// <typeparam name="T">Type of the object to wait on, and result of the returned wait condition.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1, T2, T3}"/>
		/// <remarks>This is Unity-only, as we don't want to depend on NUnit in the netstandard version.</remarks>
		[Pure]
		public static ITestWaitCondition<T> WaitForConstraint<T>(
			string objectDescription,
			Func<T> getObject,
			IResolveConstraint constraint,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new ConstraintWaitCondition<T>(
				objectDescription,
				getObject,
				constraint,
				new SourceContext(nameof(WaitForConstraint), memberName, sourceFilePath, sourceLineNumber));


		/// <summary>
		/// **Unity-only!**
		///
		/// Constructs a wait condition, which will start the provided coroutine when executed.
		/// Will complete when the coroutine has terminated.
		/// See also <seealso cref="WaitForCoroutineMethod"/>.
		/// </summary>
		/// <returns>Wait condition, which completes once the coroutine has terminated.</returns>
		/// <remarks>
		///	May be used with local functions and lambdas, as the description is manually provided.
		/// </remarks>
		/// <param name="startCoroutine">Function to start the coroutine to be waited for.</param>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithDescription{T}"/>
		[Pure]
		public static ITestWaitCondition<object> WaitForCoroutine(
			string description,
			Func<IEnumerator> startCoroutine,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		=> new CoroutineWaitCondition(
			description,
			startCoroutine,
			new SourceContext(nameof(WaitForCoroutine), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// **Unity-only!**
		///
		/// Constructs a wait condition, which will start the provided coroutine when executed.
		/// Will complete when the coroutine has terminated.
		/// The description will be the coroutine method name.
		/// See also <seealso cref="WaitForCoroutine"/>.
		/// </summary>
		/// <returns>Wait condition, which completes once the coroutine has terminated.</returns>
		/// <param name="coroutineMethod">Method to start the coroutine to be waited for.</param>
		/// <remarks>
		/// If used with a lambda or local function, you will get a weird compiler-generated description.
		/// </remarks>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T}"/>
		[Pure]
		public static ITestWaitCondition<object> WaitForCoroutineMethod(
			Func<IEnumerator> coroutineMethod,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new CoroutineWaitCondition(
				coroutineMethod.Method.Name,
				coroutineMethod,
				new SourceContext(nameof(WaitForCoroutineMethod), memberName, sourceFilePath, sourceLineNumber));

	}
}
