using Degg.Analytics;
using Sandbox;
using static CitySim.GenericTile;

namespace CitySim
{
	partial class Pawn : AnimatedEntity
	{

		[ConCmd.Server]
		public static void PlaceTile( string tileData )
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
			if ( !Game.IsServer )
			{
				return;
			}

			var isSandbox = MyGame.GameObject.CurrentGameOptions.Mode == MyGame.GameModes.Sandbox;

			TileController controller = TileController.GetTileControllerForType( SelectedTileType );
			controller.Deserialize( rawTileData );

			var canPlaceTile = isSandbox || (SelectedTileType != TileTypeEnum.Base);
			var didPlaceTile = false;


			if ( controller.CanAddToTile( tile ) && canPlaceTile )
			{
				tile.AddController( controller );
				didPlaceTile = true;
				PlaySoundClientSide( "physics.wood.impact" );
			}
			else
			{
				PlaySoundClientSide( "ui.navigate.deny" );
			}

			if ( !isSandbox )
			{
				SelectedTileType = GenericTile.TileTypeEnum.Base;
			}
			else
			{
				SelectNextTile( SelectedTileType );
			}

			if ( didPlaceTile )
			{
				var clientScore = Client.GetInt( "score", 0 );

				var score = tile.GetTileScore();
				if ( score <= 0 )
				{
					score = 0;
				}

				score = score + 1;

				clientScore = clientScore + score;

				Client.SetInt( "score", clientScore );
				RefreshSelectedTileType();
			}
		}

		public void ServerSimulate( IClient cl )
		{
			if ( DisabledControls && false )
			{
				return;
			}
			// Gameplay Controls
			if ( MyGame.GameState == MyGame.GameStateEnum.Playing )
			{
				// Input Actions 
				if ( Game.IsServer )
				{
					// Right Click or LT
					if ( Input.Pressed( InputButton.SecondaryAttack ) )
					{

						if ( SelectedTileType != TileTypeEnum.Base )
						{
							PlaySoundClientSide( "ui.navigate.back" );
							GameAnalytics.TriggerEvent( Client.SteamId.ToString(), "tile_place", -2 );
						}

						SelectedTileType = TileTypeEnum.Base;
					}

					if ( MyGame.GameObject.CurrentGameOptions.Mode == MyGame.GameModes.Sandbox )
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
