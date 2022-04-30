using System;
using NUnit.Framework.Interfaces;
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
		public TestCommand Wrap(TestCommand command) =>
#if !UNITY_2021_3
			command;
#else
			new SuppressExecutionFlowCommand(command);

		private class SuppressExecutionFlowCommand : DelegatingTestCommand
		{
			public SuppressExecutionFlowCommand(TestCommand innerCommand)
				: base(innerCommand)
			{
			}

			public override NUnit.Framework.Internal.TestResult Execute(
				NUnit.Framework.Internal.ITestExecutionContext context)
			{
				using (System.Threading.ExecutionContext.SuppressFlow())
				{
					return this.innerCommand.Execute(context);
				}
			}
		}
#endif
	}
}
