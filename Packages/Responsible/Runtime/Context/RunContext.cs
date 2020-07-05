namespace Responsible.Context
{
	public readonly struct RunContext
	{
		// This is private to enforce appending of current context!
		private readonly SourceContext sourceContext;

		internal readonly TestInstructionExecutor Executor;

		internal SourceContext SourceContext(SourceContext current) => this.sourceContext.Append(current);

		internal RunContext(TestInstructionExecutor executor, SourceContext sourceContext)
		{
			this.Executor = executor;
			this.sourceContext = sourceContext;
		}
	}
}