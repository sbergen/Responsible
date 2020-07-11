using JetBrains.Annotations;
using Responsible.State;
using UniRx;

namespace Responsible
{
	/// <summary>
	/// Represents one or more test responders, which can optionally execute when they are ready.
	/// </summary>
	public interface IOptionalTestResponder : ITestOperationContext
	{
		/// <summary>
		/// When subscribed to, starts waiting for all conditions, and publishes instructions
		/// when their condition has been met.
		/// </summary>
		[Pure]
		IOperationState<IOperationState<Unit>> CreateState();
	}
}