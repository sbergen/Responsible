namespace Responsible.Tests.Utilities
{
	public interface IConditionResponder
	{
		bool MayRespond { get; set; }
		bool MayComplete { get; set; }

		bool StartedToRespond { get; }
		bool CompletedRespond { get; }

		void AllowFullCompletion();
	}
}
