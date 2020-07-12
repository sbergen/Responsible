using System;
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
				if (previous != NotExecuted.Instance)
				{
					throw new InvalidOperationException(
						$"Can not go back to not waiting from {previous.GetType().Name}");
				}

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
				if (previous is Waiting waiting)
				{
					this.elapsedTime = waiting.WaitContext.ElapsedTime;
					waiting.WaitContext.WaitCompleted();
				}
				else
				{
					throw new InvalidOperationException(
						$"Can not transition from {previous.GetType().Name} to completed");
				}
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
				if (previous is Waiting waiting)
				{
					this.elapsedTime = waiting.WaitContext.ElapsedTime;
					this.Error = error;
					this.SourceContext = sourceContext;
				}
				else
				{
					throw new InvalidOperationException(
						$"Can not transition from {previous.GetType().Name} to failed");
				}
			}

			public override string MakeStatusLine(string description) =>
				$"[!] {description} (Failed after {this.elapsedTime})";
		}
	}
}