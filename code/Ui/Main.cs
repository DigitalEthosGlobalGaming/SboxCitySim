using Sandbox;
using Sandbox.UI;

namespace CitySim.UI
{
	public partial class Main : Panel
	{
		public Panel Base { get; set; }
		public Label CurrentItem { get; set; }
		public Label CurrentScore { get; set; }
		public Label GameOverTime { get; set; }
		public GameControlInfoUI gameControlInfoUI { get; set; }
		public Main()
		{
			SetTemplate( "Ui/Main.html" );
			StyleSheet.Load( "/Ui/Main.scss" );
			gameControlInfoUI = AddChild<GameControlInfoUI>( "gamecontrolinfoui_root" );
		}

		public override void Tick()
		{
			var player = Game.LocalPawn as Pawn;

			if ( player != null )
			{
				var selectedTile = player.SelectedTileType;
				if ( selectedTile == GenericTile.TileTypeEnum.Base )
				{
					CurrentItem.Text = "Empty";
				}
				else
				{
					CurrentItem.Text = player.SelectedTileType.ToString();
				}


				var map = MyGame.GetMap();
				var score = Game.LocalClient.GetInt( "score", 0 );

				CurrentScore.Text = (score).ToString();
			}
		}

	}
}
