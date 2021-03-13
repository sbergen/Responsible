using System;
using Responsible.State;

namespace Responsible.Tests.Editor
{
	internal class FakeOperationState : ITestOperationState
	{
		public readonly string StringRepresentation;
		public readonly Exception Exception;

		public FakeOperationState(string stringRepresentation)
		{
			this.StringRepresentation = stringRepresentation;
		}

		public FakeOperationState(Exception exception)
		{
			this.Exception = exception;
		}

		public TestOperationStatus Status => throw new NotImplementedException();
		public void BuildDescription(StateStringBuilder builder) => throw new NotImplementedException();

		public override string ToString() => this.Exception != null
			? throw this.Exception
			: this.StringRepresentation;
	}
}
