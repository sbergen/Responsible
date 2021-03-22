using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Responsible.Unity
{
	public class UnityErrorLogInterceptor : IExternalResultSource
	{
		private readonly List<(LogType type, Regex regex)> expectedLogs = new List<(LogType, Regex)>();

		/// <summary>
		/// Expect a log message during execution of any subsequent test instructions.
		/// Similar to <see cref="LogAssert.Expect(LogType, Regex)"/>,
		/// but needs to be called instead of that, in order to work when executing test instructions.
		/// </summary>
		/// <remarks>
		/// Will call <see cref="LogAssert.Expect(LogType, Regex)"/>, so it doesn't need to be called separately.
		/// </remarks>
		/// <param name="logType">Log type to expect.</param>
		/// <param name="regex">Regex to match the message to be expected.</param>
		public void ExpectLog(LogType logType, Regex regex)
		{
			LogAssert.Expect(logType, regex);
			this.expectedLogs.Add((logType, regex));
		}

		// Intercept errors respecting and not toggling the globally mutable (yuck) LogAssert.ignoreFailingMessages
		public async Task<T> GetExternalResult<T>(CancellationToken cancellationToken)
		{
			var completionSource = new TaskCompletionSource<T>();

			void HandleSingleLog(string condition, LogType type)
			{
				var index = this.expectedLogs.FindIndex(entry =>
					entry.type == type &&
					entry.regex.IsMatch(condition));

				if (index >= 0)
				{
					// Already expected, just remove it
					this.expectedLogs.RemoveAt(index);
				}
				else
				{
					LogAssert.Expect(type, condition);
					completionSource.SetException(new UnhandledLogMessageException(condition));
				}
			}

			void LogHandler(string condition, string _, LogType type)
			{
				if (!LogAssert.ignoreFailingMessages &&
					(type == LogType.Error || type == LogType.Exception))
				{
					HandleSingleLog(condition, type);
				}
			}

			using (cancellationToken.Register(() => completionSource.SetCanceled()))
			{
				Application.logMessageReceived += LogHandler;
				try
				{
					return await completionSource.Task;
				}
				finally
				{
					Application.logMessageReceived -= LogHandler;
				}
			}
		}
	}
}
