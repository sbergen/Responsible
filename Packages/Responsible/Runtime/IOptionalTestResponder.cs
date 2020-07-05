using System;
using Responsible.Context;
using UniRx;

namespace Responsible
{
	public interface IOptionalTestResponder : ITestOperationContext
	{
		IObservable<IObservable<Unit>> Instructions(RunContext runContext, WaitContext waitContext);
	}
}