using System;
using JetBrains.Annotations;
using Responsible.Context;
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
		IObservable<IObservable<Unit>> Instructions(RunContext runContext, WaitContext waitContext);
	}
}