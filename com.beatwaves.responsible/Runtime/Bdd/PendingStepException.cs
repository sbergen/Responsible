using System;

namespace Responsible.Bdd
{
	internal class PendingStepException : OperationCanceledException
	{
		internal const string PendingMessage = "BDD step implementation pending";

		internal PendingStepException()
			: base(PendingMessage)
		{
		}
	}
}
