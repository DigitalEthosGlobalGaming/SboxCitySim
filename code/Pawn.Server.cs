using Sandbox;
using System.Collections.Generic;
using System.Text.Json;

namespace CitySim
{
	partial class Pawn : AnimEntity
	{

		[ServerCmd]
		public static void PlaceTile(string tileData )
		{
			var player = ConsoleSystem.Caller.Pawn as Pawn;
			var tile = player.GetTileLookedAt();
			if ( tile != null )
			{
				player.PlaceTile( tile, tileData );
			}
		}

		public void PlaceTile( GenericTile tile, string rawTileData )
		{
			if (!IsServer)
			{
				return;
			}

			TileController controller = TileController.GetTileControllerForType( SelectedTileType );
			controller.Deserialize( rawTileData );

			if ( controller.CanAddToTile( tile ) )
			{
				tile.AddController( controller );
				PlaySoundClientSide( "physics.wood.impact" );
			}
			else
			{
				PlaySoundClientSide( "ui.navigate.deny" );
			}

			if ( MyGame.CurrentGameOptions.Mode != MyGame.GameModes.Sandbox )
			{
				SelectedTileType = GenericTile.TileTypeEnum.Base;
			}
			else
			{
				SelectNextTile( SelectedTileType );
			}

			var clientScore = Client.GetInt( "score", 0 );

			var score = tile.GetTileScore();
			if ( score <= 0)
			{
				score = 0;
			}

			score = score + 1;

			clientScore = clientScore + score;

			Client.SetInt( "score", clientScore );
			RefreshSelectedTileType();
		}

	}
}
