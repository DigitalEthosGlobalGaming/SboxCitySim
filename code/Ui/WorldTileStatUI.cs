using Degg.Ui.Elements;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace CitySim.UI
{
	public class WorldTileStatUI : Billboard
	{
		public static List<WorldTileStatUI> All { get; private set; } = new List<WorldTileStatUI>();

		public Panel Base { get; set; }
		private string name;
		public string Name { 
			get { return name; }
			set
			{
				if (nameLabel != null)
					nameLabel.Text = value;
				name = value;
			} 
		}
		private int points;
		public int Points
		{
			get { return points; }
			set
			{
				if (pointsLabel != null)
					pointsLabel.Text = value.ToString();
				points = value;
			}
		}

		public Label nameLabel;
		public Label pointsLabel;

		public WorldTileStatUI()
		{	
			SetTemplate( "Ui/WorldTileStatUi.html" );
			StyleSheet.Load( "Ui/WorldTileStatUi.scss" );

			pointsLabel = Base.Add.Label( "0" );
		}
		
		public override void Tick()
		{
			base.Tick();
			var scale = MathX.Clamp( Scale, 0.5f, 4f ) * 10;
			scale = MathX.CeilToInt( scale ) / 10f;

			pointsLabel.Style.Set( "font-size", $"{ 0.5f * scale}vw" );
			Style.Set( "border-width", $"{4*scale}px" );
			Style.Set( "padding", $"{6*scale}px" );

			SetClass( "bad", points < 0 );
			SetClass( "good", points > 0 );
		}
	}
}
