using System;
using JetBrains.Annotations;
using Responsible.Context;

namespace Responsible
{
	/// <summary>
	/// Represents a test instruction which can be executed.
	/// Test instructions can be chained, and usually have an internal timeout.
	/// </summary>
	public interface ITestInstruction<out T> : ITestOperationContext
	{
		[Pure]
		IObservable<T> Run(RunContext runContext);
	}
}