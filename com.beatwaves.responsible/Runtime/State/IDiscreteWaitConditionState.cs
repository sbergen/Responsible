using System;

namespace Responsible.State
{
	/// <summary>
	/// A discrete, or singular, wait state, which can be combined with
	/// expect within operations in output.
	/// </summary>
	internal interface IDiscreteWaitConditionState : ITestOperationState
	{
		string Description { get; }
		Action<StateStringBuilder>? ExtraContext { get; }
	}
}
