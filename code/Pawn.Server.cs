using Degg.Analytics;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json;
using static CitySim.GenericTile;

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

		public void ServerSimulate( Client cl )
		{
			if ( DisabledControls && false )
			{
				return;
			}
			// Gameplay Controls
			if ( MyGame.GameState == MyGame.GameStateEnum.Playing )
			{
				// Input Actions 
				if ( IsServer )
				{
					// Right Click or LT
					if ( Input.Pressed( InputButton.Attack2 ) )
					{

						if ( SelectedTileType != TileTypeEnum.Base )
						{
							PlaySoundClientSide( "ui.navigate.back" );
							GameAnalytics.TriggerEvent( Client.PlayerId.ToString(), "tile_place", -2 );
						}

						SelectedTileType = TileTypeEnum.Base;
					}

					if ( MyGame.CurrentGameOptions.Mode == MyGame.GameModes.Sandbox )
					{
						GenericTile.TileTypeEnum? tileToSelect = null;

						// Controls for Sandbox GameMode
						if ( Input.Pressed( InputButton.Slot1 ) )
						{
							tileToSelect = GenericTile.TileTypeEnum.Business;
						}
						if ( Input.Pressed( InputButton.Slot2 ) )
						{
							tileToSelect = GenericTile.TileTypeEnum.Park;
						}
						if ( Input.Pressed( InputButton.Slot3 ) )
						{
							tileToSelect = GenericTile.TileTypeEnum.House;
						}
						if ( Input.Pressed( InputButton.Slot4 ) )
						{
							tileToSelect = GenericTile.TileTypeEnum.Road;
						}

						if ( tileToSelect != null )
						{
							PlaySoundClientSide( "ui.button.press" );
							SelectNextTile( tileToSelect.Value );
						}
					}
				}
			}
		}
	}
}
