using System;
using System.Threading;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace Responsible.Tests.Utilities
{
	/// <summary>
	/// Applies workarounds required for Unity 2021.3, where task error handling is asynchronous.
	/// This seems like a bug, but I can't be sure until I report it to Unity...
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class TaskExceptionTestAttribute : Attribute, IWrapSetUpTearDown
	{
		public TestCommand Wrap(TestCommand command) => new SuppressExecutionFlowCommand(command);

		private class SuppressExecutionFlowCommand : DelegatingTestCommand
		{
			public SuppressExecutionFlowCommand(TestCommand innerCommand)
				: base(innerCommand)
			{
			}

			public override TestResult Execute(ITestExecutionContext context)
			{
				using (ExecutionContext.SuppressFlow())
				{
					return this.innerCommand.Execute(context);
				}
			}
		}
	}
}
