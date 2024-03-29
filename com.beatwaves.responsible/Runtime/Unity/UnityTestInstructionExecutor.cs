using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Responsible.Unity
{
	/// <summary>
	/// A <see cref="TestInstructionExecutor"/> implementation that should work well in Unity:
	/// * Polls on every frame
	/// * Causes test operations to fail on logged errors
	/// * Can also log failures, as Unity will swallow exceptions in nested coroutines
	/// </summary>
	/// <remarks>
	/// Remember to dispose all instances of this class which you create!
	/// </remarks>
	public class UnityTestInstructionExecutor : TestInstructionExecutor
	{
		private readonly UnityTestScheduler scheduler;
		private readonly UnityErrorLogInterceptor errorLogInterceptor;

		private UnityTestInstructionExecutor(
			UnityTestScheduler scheduler,
			UnityErrorLogInterceptor errorLogInterceptor,
			bool logErrors,
			IGlobalContextProvider globalContextProvider)
			: base(
				scheduler,
				errorLogInterceptor,
				logErrors ? new UnityFailureListener() : null,
				globalContextProvider,
				new[]
				{
					typeof(IgnoreException),
					typeof(InconclusiveException),
					typeof(SuccessException),
				})
		{
			this.scheduler = scheduler;
			this.errorLogInterceptor = errorLogInterceptor;
		}

		/// <summary>
		/// Similar to <see cref="LogAssert.Expect(LogType, Regex)"/>, but **must** be used instead of that,
		/// for Responsible to be able to successfully ignore errors.
		/// See also <seealso cref="UnityErrorLogInterceptor"/>.
		/// <see cref="LogAssert.ignoreFailingMessages"/> is respected
		/// </summary>
		/// <param name="logType">Log type to expect</param>
		/// <param name="regex">Regular expression to match in the expected message</param>
		public void ExpectLog(LogType logType, Regex regex) => this.errorLogInterceptor.ExpectLog(logType, regex);

		/// <summary>
		/// Constructs a new TestInstructionExecutor with a default Unity configuration.
		/// If you are not using Responsible all the way, but also have some bare coroutines involved,
		/// you will probably want to also log any errors, in addition to failing the tests the normal way,
		/// as Unity has the tendency to swallow exceptions from nested coroutines.
		/// </summary>
		/// <param name="logErrors">Whether or not errors should be logged in addition to propagated.</param>
		/// <param name="globalContextProvider">
		/// Optional provider for global context, which gets included in failure messages.
		/// </param>
		public UnityTestInstructionExecutor(bool logErrors = true, IGlobalContextProvider globalContextProvider = null)
			: this(UnityTestScheduler.Create(), new UnityErrorLogInterceptor(), logErrors, globalContextProvider)
		{
		}

		/// <summary>
		/// Disposes this executor, so that no more polling or log intercepting is done.
		/// </summary>
		public override void Dispose()
		{
			this.scheduler.Dispose();
			base.Dispose();
		}
	}
}
