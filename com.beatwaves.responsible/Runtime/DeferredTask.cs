using System.Threading;
using System.Threading.Tasks;

namespace Responsible
{
	internal delegate Task<T> DeferredTask<T>(CancellationToken cancellationToken);
}
