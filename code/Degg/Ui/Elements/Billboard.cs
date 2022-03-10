using Sandbox;
using Sandbox.UI;


namespace Degg.Ui.Elements
{
	public class Billboard :  Panel
	{
		public Vector3 Position { get; set; }

		public Billboard()
		{

		}

		public void SetPosition(Vector3 position)
		{
			var panelPos = position.ToScreen();

			var left = panelPos.x * 100;
			Style.Left = Length.Percent( left );

			var top = panelPos.y * 100;
			Style.Top = Length.Percent( top );

			Style.Position = PositionMode.Absolute;
		}
		
	}
}
