using System.Threading.Tasks;
using FluentAssertions;

namespace Responsible.Tests.Utilities
{
	internal static class TaskExtensions
	{
		public static T AssertSynchronousResult<T>(this Task<T> task)
		{
			task.Wait(0).Should().BeTrue("expecting synchronous result in task");
			return task.Result;
		}
	}
}
