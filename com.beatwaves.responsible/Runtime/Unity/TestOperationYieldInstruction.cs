using System;
using System.Diagnostics.CodeAnalysis;
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

		/// <summary>Indicates if the test operation has completed with an error.</summary>
		/// <value>True if the test operation completed with an error, false otherwise.</value>
		public bool CompletedWithError => this.task.IsFaulted && !this.WasCanceled;

		/// <summary>Indicates if the test operation was canceled.</summary>
		/// <value>True if the test operation was canceled, false otherwise.</value>
		public bool WasCanceled =>
			this.task.IsFaulted &&
			this.GetException().InnerException is TaskCanceledException;

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
		/// Gets the TestFailureException that caused the test operation to fail.
		/// Will throw an error if the task has not failed or was not canceled.</summary>
		/// <value>The exception that caused the test operation to fail.</value>
		public TestFailureException Error => this.GetException();

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
					// ReSharper disable once PossibleNullReferenceException, Condition is checked above...
					throw this.GetException();
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

		private TestFailureException GetException()
		{
			if (!this.task.IsFaulted)
			{
				throw new InvalidOperationException("Test operation has not failed!");
			}
			else
			{
				return this.ExpectTestFailureException(this.task.Exception);
			}
		}

		// Should never fail, unless we have a bug in our code,
		// which means it should be caught by other tests.
		// We don't test internals directly in Responsible.
		[ExcludeFromCodeCoverage]
		private TestFailureException ExpectTestFailureException(Exception e)
		{
			if (e is AggregateException aggregateException &&
				aggregateException.InnerExceptions.Count == 1 &&
				aggregateException.InnerExceptions[0] is TestFailureException failureException)
			{
				return failureException;
			}
			else
			{
				throw e;
			}
		}
	}
}
