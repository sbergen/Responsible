using System.Collections.Generic;
using System.Linq;

namespace Responsible.Context
{
	public abstract class ContextBase
	{
		private readonly List<(ITestOperationContext from, ITestOperationContext to)> relations =
			new List<(ITestOperationContext, ITestOperationContext)>();

		internal IEnumerable<ITestOperationContext> RelatedContexts(ITestOperationContext context) =>
			this.relations.Where(r => r.from == context).Select(r => r.to);

		public void AddRelation(ITestOperationContext from, ITestOperationContext to) =>
			this.relations.Add((from, to));
	}
}