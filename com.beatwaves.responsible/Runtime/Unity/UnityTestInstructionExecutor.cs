using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Responsible.Unity
{
	public class UnityTestInstructionExecutor : TestInstructionExecutor
	{
		private readonly UnityTimeProvider timeProvider;
		private readonly UnityErrorLogInterceptor errorLogInterceptor = new UnityErrorLogInterceptor();

		public void ExpectLog(LogType logType, Regex regex) => this.errorLogInterceptor.ExpectLog(logType, regex);

		private UnityTestInstructionExecutor(UnityTimeProvider timeProvider, bool logErrors)
			: base(
				timeProvider,
				new UnityErrorLogInterceptor(),
				logErrors ? new UnityFailureListener() : null)
		{
			this.timeProvider = timeProvider;
		}

		/// <summary>
		/// Constructs a new TestInstructionExecutor with a default Unity configuration.
		/// If you are not using Responsible all the way, but also have some bare coroutines involved,
		/// you will probably want to also log any errors, in addition to failing the tests the normal way,
		/// as Unity has the tendency to swallow exceptions from coroutines at times.
		/// </summary>
		/// <param name="logErrors">Whether or not errors should be logged in addition to propagated.</param>
		public UnityTestInstructionExecutor(bool logErrors = true)
			: this(CreateTimeProvider(), logErrors)
		{
		}

		public override void Dispose()
		{
			Object.DestroyImmediate(this.timeProvider.gameObject);
			base.Dispose();
		}

		private static UnityTimeProvider CreateTimeProvider()
		{
			var go = new GameObject();
			Object.DontDestroyOnLoad(go);
			return go.AddComponent<UnityTimeProvider>();
		}
	}
}
