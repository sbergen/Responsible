using System;

namespace Responsible.Utilities
{
	internal static class Disposable
	{
		public static IDisposable Create(Action disposeAction) => new ActionDisposable(disposeAction);

		private class ActionDisposable : IDisposable
		{
			private readonly Action disposeAction;
			private bool isDisposed;

			public ActionDisposable(Action disposeAction)
				=> this.disposeAction = disposeAction;

			public void Dispose()
			{
				if (!this.isDisposed)
				{
					this.isDisposed = true;
					this.disposeAction();
				}
			}
		}
	}
}
