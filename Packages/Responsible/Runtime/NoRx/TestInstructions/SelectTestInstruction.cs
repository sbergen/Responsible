using System;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;

namespace Responsible.NoRx.TestInstructions
{
	internal class SelectTestInstruction<T1, T2> : TestInstructionBase<T2>
	{
		public SelectTestInstruction(
			ITestInstruction<T1> first,
			Func<T1, T2> selector,
			SourceContext sourceContext)
			: base(() => new SelectOperationState<T1,T2>(first.CreateState(), selector, sourceContext))
		{
		}
	}
}
