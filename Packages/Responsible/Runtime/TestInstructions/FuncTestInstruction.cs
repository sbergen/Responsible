using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	public class FuncTestInstruction<T> : ITestInstruction<T>
	{
		private readonly Func<T> create;

		public FuncTestInstruction(Func<T> create)
		{
			this.create = create;
		}

		public IObservable<T> Run(RunContext runContext) =>
			Observable.Defer(() => Observable.Return(this.create()));
	}
}