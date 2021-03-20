using System.Threading;
using System.Threading.Tasks;
using Responsible.NoRx.Context;

namespace Responsible.NoRx.State
{
	public interface ITestOperationState
	{
		TestOperationStatus Status { get; }
		void BuildDescription(StateStringBuilder builder);
	}

	public interface ITestOperationState<T> : ITestOperationState
	{
		Task<T> Execute(RunContext runContext, CancellationToken cancellationToken);
	}
}