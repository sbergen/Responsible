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
	public class TaskYieldInstruction<T> : CustomYieldInstruction
	{
		private readonly Task<T> task;

		/// <summary>Indicates if the test operation has completed with an error.</summary>
		/// <value>True if the test operation completed with an error, false otherwise.</value>
		public bool HasError => this.task.IsFaulted;

		/// <summary>Indicates if the test operation was canceled.</summary>
		/// <value>True if the test operation was canceled, false otherwise.</value>
		public bool IsCanceled => this.task.IsCanceled;

		/// <summary>Indicates if the test operation has completed successfully.</summary>
		/// <value>True if the test operation completed successfully, false otherwise.</value>
		public bool IsCompleted => this.task.IsCompleted;

		/// <summary>Indicates if the test operation completed with an error.</summary>
		/// <value>True if the test operation completed with an error, false otherwise.</value>
		/// <exception cref="InvalidOperationException">The test operation was not completed successfully.</exception>
		public T Result => this.IsCompleted
			? this.task.Result
			: throw new InvalidOperationException("Test operation has not completed successfully!");

		/// <summary>The exception that caused the test operation to fail.</summary>
		/// <value>TODO: make this check that we actually failed, and get the correct error!</value>
		public Exception Error => this.task.Exception;

		internal TaskYieldInstruction(Task<T> task)
		{
			this.task = task;
		}

		/// <summary>
		/// Implementation of <see cref="CustomYieldInstruction"/>, not to be used for other purposes.
		/// </summary>
		public override bool keepWaiting => this.KeepWaiting();

		private bool KeepWaiting()
		{
			if (this.task.IsFaulted)
			{
				// ReSharper disable once PossibleNullReferenceException, Condition is checked above...
				throw this.task.Exception;
			}
			else
			{
				return !this.task.IsCompleted && !this.task.IsCanceled;
			}
		}
	}
}
