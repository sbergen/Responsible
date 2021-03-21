using System.Threading.Tasks;
using NUnit.Framework;

namespace Responsible.Tests.Runtime.NoRx.Utilities
{
	internal static class TaskExtensions
	{
		public static T AssertSynchronousResult<T>(this Task<T> task)
		{
			Assert.IsTrue(task.Wait(0), "Expecting synchronous result in task");
			return task.Result;
		}
	}
}
