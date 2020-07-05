using System;
using JetBrains.Annotations;
using Responsible.Context;
using UniRx;

namespace Responsible
{
	public interface IOptionalTestResponder : ITestOperationContext
	{
		[Pure]
		IObservable<IObservable<Unit>> Instructions(RunContext runContext, WaitContext waitContext);
	}
}