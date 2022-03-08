using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace CitySim.UI
{
	public class WorldTileStatUI : WorldPanel
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
			All.Add( this );

			var size = 1000;
			PanelBounds = new Rect( -size, -size, size, size );
			SetTemplate( "Ui/WorldTileStatUi.html" );
			StyleSheet.Load( "Ui/WorldTileStatUi.scss" );
			AddClass( "stat-ui" );
			AddClass( "open" );


			pointsLabel = Base.Add.Label( "0" );
		}
		
		public override void Tick()
		{
			base.Tick();

			SetClass( "bad", points < 0 );
			SetClass( "good", points > 0 );
			// Closest calculation for now.... there's a bug here.
			Rotation = Rotation.LookAt( (Local.Pawn.Position - Transform.Position).Normal );
		}
	}
}
