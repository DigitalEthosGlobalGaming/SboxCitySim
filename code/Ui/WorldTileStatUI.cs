using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CitySim.UI
{
	public class WorldTileStatUI : WorldPanel
	{
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
		private string points;
		public string Points
		{
			get { return points; }
			set
			{
				if (pointsLabel != null)
					pointsLabel.Text = value;
				points = value;
			}
		}

		public Label nameLabel;
		public Label pointsLabel;

		public WorldTileStatUI()
		{
			Style.FontColor = Color.White;
			Style.FontSize = Length.Pixels(64);
			nameLabel = Add.Label( "Unknown Name" );
			pointsLabel = Add.Label( "0" );
		}

		public override void Tick()
		{
			base.Tick();


			// Closest calculation for now.... there's a bug here.
			Rotation = Rotation.LookAt( (Local.Pawn.Position - Transform.Position).Normal );
		}
	}
}
