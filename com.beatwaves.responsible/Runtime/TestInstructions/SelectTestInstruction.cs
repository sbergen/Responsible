using System;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestInstructions
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
