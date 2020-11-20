using System;
using System.Diagnostics.CodeAnalysis;
using Responsible.Context;

namespace Responsible.State
{
	public abstract class TestOperationStatus
	{
		public abstract string MakeStatusLine(string description);

		internal class NotExecuted : TestOperationStatus
		{
			public static readonly NotExecuted Instance = new NotExecuted();

			private NotExecuted()
			{
			}

			public override string MakeStatusLine(string description) =>
				$"[ ] {description}";
		}

		internal class Waiting : TestOperationStatus
		{
			internal readonly WaitContext WaitContext;

			public Waiting(TestOperationStatus previous, WaitContext waitContext)
			{
				AssertNotStarted(previous);
				this.WaitContext = waitContext;
			}

			public override string MakeStatusLine(string description) =>
				$"[.] {description} (Started {this.WaitContext.ElapsedTime} ago)";
		}

		internal class Completed : TestOperationStatus
		{
			private readonly string elapsedTime;

			public Completed(TestOperationStatus previous)
			{
				var waiting = this.ExpectWaiting(previous);
				this.elapsedTime = waiting.WaitContext.ElapsedTime;
				waiting.WaitContext.WaitCompleted();
			}

			public override string MakeStatusLine(string description) =>
				$"[✓] {description} (Completed in {this.elapsedTime})";
		}

		internal class Failed : TestOperationStatus
		{
			private readonly string elapsedTime;

			public readonly Exception Error;
			public readonly SourceContext SourceContext;

			public Failed(TestOperationStatus previous, Exception error, SourceContext sourceContext)
			{
				var waiting = this.ExpectWaiting(previous);
				this.elapsedTime = waiting.WaitContext.ElapsedTime;
				this.Error = error;
				this.SourceContext = sourceContext;
			}

			public override string MakeStatusLine(string description) =>
				$"[!] {description} (Failed after {this.elapsedTime})";
		}

		internal class Canceled : TestOperationStatus
		{
			private readonly string elapsedTime;

			public Canceled(TestOperationStatus previous)
			{
				var waiting = this.ExpectWaiting(previous);
				this.elapsedTime = waiting.WaitContext.ElapsedTime;
			}

			public override string MakeStatusLine(string description) =>
				$"[-] {description} (Canceled after {this.elapsedTime})";
		}

		[ExcludeFromCodeCoverage] // Unreachable defensive code
		public static void AssertNotStarted(TestOperationStatus status)
		{
			if (status != NotExecuted.Instance)
			{
				throw new InvalidOperationException(
					$"Operation already started ({status?.GetType().Name})");
			}
		}

		[ExcludeFromCodeCoverage] // Unreachable defensive code
		private Waiting ExpectWaiting(TestOperationStatus status)
		{
			if (status is Waiting waiting)
			{
				return waiting;
			}
			else
			{
				throw new InvalidOperationException(
					$"Can not transition from {status.GetType().Name} to {this.GetType().Name}");
			}
		}
	}
}