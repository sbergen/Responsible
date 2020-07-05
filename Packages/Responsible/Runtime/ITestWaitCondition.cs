using System;
using JetBrains.Annotations;
using Responsible.Context;

namespace Responsible
{
	public interface ITestWaitCondition<out T> : ITestOperationContext
	{
		[Pure]
		IObservable<T> WaitForResult(RunContext runContext, WaitContext waitContext);
	}
}