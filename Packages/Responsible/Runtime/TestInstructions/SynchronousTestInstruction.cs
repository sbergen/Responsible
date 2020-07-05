using System;
using Responsible.Context;
using UniRx;
using UnityEngine;

namespace Responsible.TestInstructions
{
	internal class SynchronousTestInstruction<T> : ITestInstruction<T>
	{
		private readonly Func<T> action;
		private readonly SourceContext sourceContext;

		public SynchronousTestInstruction(Func<T> action, SourceContext sourceContext)
		{
			this.action = action;
			this.sourceContext = sourceContext;
		}

		public IObservable<T> Run(RunContext runContext)
		{
			try
			{
				return Observable.Return(this.action());
			}
			catch (Exception e)
			{
				runContext.Executor.Logger.Log(
					LogType.Error,
					string.Join(
						TestInstructionExecutor.UnityEmptyLine,
						$"Synchronous test action failed: {e}",
						TestInstructionExecutor.InstructionStack(runContext.SourceContext(this.sourceContext))));
				throw;
			}
		}
	}
}