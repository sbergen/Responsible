using System;
using System.Collections.Generic;
using System.Linq;

namespace Responsible.Context
{
	public class RunContext : ContextBase
	{
		private readonly List<(ITestOperationContext context, Exception error)> failedOperations =
			new List<(ITestOperationContext, Exception)>();

		// This is private to enforce appending of current context!
		private readonly SourceContext sourceContext;

		internal readonly TestInstructionExecutor Executor;

		public void MarkAsFailed(ITestOperationContext context, Exception e) =>
			this.failedOperations.Add((context, e));

		internal Exception ErrorIfFailed(ITestOperationContext context) =>
			this.failedOperations
				.Where(failContext => failContext.context == context)
				.Select(failContext => failContext.error)
				.FirstOrDefault();

		internal SourceContext SourceContext(SourceContext current) => this.sourceContext.Append(current);

		internal RunContext(TestInstructionExecutor executor, SourceContext sourceContext)
		{
			this.Executor = executor;
			this.sourceContext = sourceContext;
		}
	}
}