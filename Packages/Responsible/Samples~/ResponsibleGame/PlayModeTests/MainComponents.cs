namespace ResponsibleGame.PlayModeTests
{
	public class MainComponents
	{
		public readonly PlayerObject PlayerObject;
		public readonly TargetArea TargetArea;

		public bool BothSet => this.PlayerObject != null && this.TargetArea != null;

		public MainComponents(PlayerObject playerObject, TargetArea targetArea)
		{
			this.PlayerObject = playerObject;
			this.TargetArea = targetArea;
		}
	}
}