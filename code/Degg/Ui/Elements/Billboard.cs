using Sandbox;
using Sandbox.UI;

namespace Degg.UI.Elements
{
	public class Billboard :  Panel
	{
		public Vector3 Position { get; set; }

		public float Scale { get; set; } = 0;

		public Billboard()
		{

		}

		public void SetScale(float scale)
		{
			this.Scale = scale;
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
