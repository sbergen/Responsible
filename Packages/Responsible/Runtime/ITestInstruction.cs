using System;
using Responsible.Context;

namespace Responsible
{
	public interface ITestInstruction<out T>
	{
		IObservable<T> Run(RunContext runContext);
	}
}