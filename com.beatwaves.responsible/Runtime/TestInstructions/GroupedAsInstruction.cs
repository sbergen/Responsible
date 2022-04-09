using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestInstructions
{
	internal class GroupedAsInstruction<T> : TestInstructionBase<T>
	{
		public GroupedAsInstruction(
			string description,
			ITestInstruction<T> instruction)
			: base(() => new State(description, instruction))
		{
		}

		private sealed class State : TestOperationState<T>
		{
			private readonly string description;
			private readonly ITestOperationState<T> state;

			public State(
				string description,
				ITestInstruction<T> instruction)
				: base(null)
			{
				this.description = description;
				this.state = instruction.CreateState();
			}

			protected override Task<T> ExecuteInner(RunContext runContext, CancellationToken cancellationToken) =>
				this.state.Execute(runContext, cancellationToken);

			public override void BuildDescription(StateStringBuilder builder)
			{
				builder.AddNestedDetails(this.description, this.state.BuildDescription);
			}
		}
	}
}
