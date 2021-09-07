using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Responsible.Unity
{
	/// <summary>
	/// A <see cref="CustomYieldInstruction"/> that can be yielded from a
	/// <c>[UnityTest]</c> playmode test.
	/// </summary>
	/// <typeparam name="T">Result type of the test operation being run.</typeparam>
	public class TestOperationYieldInstruction<T> : CustomYieldInstruction
	{
		private readonly Task<T> task;
		private readonly bool throwOnError;

		/// <summary>Indicates if the test operation was canceled.</summary>
		/// <value>True if the test operation was canceled, false otherwise.</value>
		public bool WasCanceled =>
			this.task.IsFaulted &&
			InnermostException(this.task.Exception) is TaskCanceledException;

		/// <summary>Indicates if the test operation has completed with an error.</summary>
		/// <value>True if the test operation completed with an error, false otherwise.</value>
		public bool CompletedWithError => this.task.IsFaulted && !this.WasCanceled;

		/// <summary>Indicates if the test operation has completed successfully.</summary>
		/// <value>True if the test operation completed successfully, false otherwise.</value>
		public bool CompletedSuccessfully => !this.task.IsFaulted && this.task.IsCompleted;

		/// <summary>Indicates if the test operation completed with an error.</summary>
		/// <value>True if the test operation completed with an error, false otherwise.</value>
		/// <exception cref="InvalidOperationException">The test operation was not completed successfully.</exception>
		public T Result => this.CompletedSuccessfully
			? this.task.Result
			: throw new InvalidOperationException("Test operation has not completed successfully!");

		/// <summary>
		/// Gets the <see cref="TestFailureException"/> that caused the test operation to fail.
		/// Will throw an error if the task has not failed or was not canceled.</summary>
		/// <value>The exception that caused the test operation to fail.</value>
		public TestFailureException Error => this.GetTestFailureException()
			?? throw new InvalidOperationException("Test operation has not failed!");

		internal TestOperationYieldInstruction(Task<T> task, bool throwOnError)
		{
			this.task = task;
			this.throwOnError = throwOnError;
		}

		/// <summary>
		/// Implementation of <see cref="CustomYieldInstruction"/>, not to be used for other purposes.
		/// </summary>
		public override bool keepWaiting => this.KeepWaiting();

		private bool KeepWaiting()
		{
			if (this.task.IsFaulted)
			{
				if (this.throwOnError)
				{
					throw this.GetTestFailureException() ?? InnermostException(this.task.Exception);
				}
				else
				{
					return false;
				}
			}
			else
			{
				return !this.task.IsCompleted && !this.task.IsCanceled;
			}
		}

		private static Exception InnermostException(Exception e)
		{
			var unwrapped = (e as AggregateException)?.InnerException ?? e;
			if (unwrapped is TestFailureException testFailureException)
			{
				return testFailureException.InnerException;
			}
			else
			{
				return unwrapped;
			}
		}

		private TestFailureException GetTestFailureException() =>
			this.task.Exception?.InnerException as TestFailureException;
	}
}
