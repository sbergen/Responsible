using System;
using JetBrains.Annotations;
using Responsible.Context;

namespace Responsible
{
	public interface ITestInstruction<out T>
	{
		[Pure]
		IObservable<T> Run(RunContext runContext);
	}
}