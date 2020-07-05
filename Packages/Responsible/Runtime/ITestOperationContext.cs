using JetBrains.Annotations;
using Responsible.Context;

namespace Responsible
{
	public interface ITestOperationContext
	{
		[Pure]
		void BuildDescription(ContextStringBuilder builder);

		[Pure]
		void BuildFailureContext(ContextStringBuilder builder);
	}
}