using Sandbox.UI;
using Sandbox;
using CitySim;
using System;

namespace GridSystem.Ui
{
	public partial class Main: Panel
	{

		public Panel Base { get; set; }
		public Label CurrentItem { get; set; }
		public Label CurrentScore { get; set; }
		public Label TimeToNewPiece { get; set; }
		public Label GameOverTime { get; set; }
		public Main()
		{

			SetTemplate( "Ui/Main.html" );
			StyleSheet.Load( "Ui/main.scss" );
		}

		public override void Tick()
		{
			var player = Local.Pawn as Pawn;

			if ( player != null)
			{
				var selectedTile = player.SelectedTileType;
				if ( selectedTile == RoadTile.TileTypeEnum.Base )
				{
					CurrentItem.Text = "Empty";
				}
				else
				{
					CurrentItem.Text = player.SelectedTileType.ToString();
				}


				var map = MyGame.GetMap();

				CurrentScore.Text = (map.Score + player.Score) + " Points";

				var timeToPeice = Math.Round(map.TimeForNewPiece - Time.Now);
				if ( timeToPeice  <= 0)
				{
					timeToPeice = 0;
				}
				TimeToNewPiece.Text = timeToPeice.ToString();
			}
		}

	}
}
