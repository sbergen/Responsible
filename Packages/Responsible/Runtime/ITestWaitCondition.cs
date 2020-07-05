using System;
using Responsible.Context;

namespace Responsible
{
	public interface ITestWaitCondition<out T> : ITestOperationContext
	{
		IObservable<T> WaitForResult(RunContext runContext, WaitContext waitContext);
	}
}