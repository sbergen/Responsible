using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Responsible.Unity
{
	public class TaskYieldInstruction<T> : CustomYieldInstruction
	{
		private readonly Task<T> task;

		public bool HasError => this.task.IsFaulted;
		public bool HasResult => this.task.IsCompleted;
		public bool IsCanceled => this.task.IsCanceled;
		public T Result => this.task.Result;
		public Exception Error => this.task.Exception;

		public TaskYieldInstruction(Task<T> task)
		{
			this.task = task;
		}

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
