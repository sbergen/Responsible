using System;
using Responsible.Context;

namespace Responsible.State
{
	public abstract class OperationStatus
	{
		public static void Update(ref OperationStatus previous, OperationStatus next)
		{
			if (next is NotExecuted)
			{
				throw new InvalidOperationException(
					$"Can not go back to not executed from {previous.GetType().Name}");
			}
			else if (next is Waiting)
			{
				if (previous is Completed || previous is Failed)
				{

				}

				previous = next;
			}
			else
			{
				if (previous is Completed || previous is Failed)
				{
					throw new InvalidOperationException(
						$"Can not transition from {previous.GetType().Name} to {next.GetType().Name}");
				}

				previous = next;
			}
		}

		public abstract string MakeStatusLine(string description);

		public class NotExecuted : OperationStatus
		{
			public static readonly NotExecuted Instance = new NotExecuted();

			private NotExecuted()
			{
			}

			public override string MakeStatusLine(string description) =>
				$"[ ] {description}";
		}

		public class Waiting : OperationStatus
		{
			internal readonly WaitContext WaitContext;

			public Waiting(OperationStatus previous, RunContext runContext)
			{
				if (previous != NotExecuted.Instance)
				{
					throw new InvalidOperationException(
						$"Can not go back to not waiting from {previous.GetType().Name}");
				}

				this.WaitContext = runContext.MakeWaitContext();
			}

			public override string MakeStatusLine(string description) =>
				$"[.] {description} (Started {this.WaitContext.ElapsedTime} ago)";
		}

		public class Completed : OperationStatus
		{
			private readonly string elapsedTime;

			public Completed(OperationStatus previous)
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

		public class Failed : OperationStatus
		{
			private readonly string elapsedTime;

			public readonly Exception Error;

			public Failed(OperationStatus previous, Exception error)
			{
				if (previous is Waiting waiting)
				{
					this.elapsedTime = waiting.WaitContext.ElapsedTime;
					this.Error = error;
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