using Responsible.Context;

namespace Responsible
{
	public interface ITestOperationContext
	{
		void BuildDescription(ContextStringBuilder builder);
		void BuildFailureContext(ContextStringBuilder builder);
	}
}