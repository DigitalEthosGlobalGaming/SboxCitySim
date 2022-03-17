﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json;
using static CitySim.GenericTile;

namespace CitySim
{
	partial class Pawn : AnimEntity
	{

		[ClientRpc]
		public void RefreshSelectedTileType( GenericTile tile = null )
		{
			var previousTileData = this?.SelectedController?.Serialize();
			var previousControllerType = this?.SelectedController?.GetTileType();

			this?.SelectedController?.SetVisible( false );

			tile = tile ?? LastHighlighted;
			if ( tile != null && SelectedTileType != TileTypeEnum.Base )
			{
				TileController controller = TileController.GetTileControllerForType( SelectedTileType );

				if ( controller.GetTileType() == previousControllerType )
				{
					controller.Deserialize( previousTileData );
				}

				if ( controller != null && controller.CanAddToTile( tile ) )
				{
					controller.Parent = tile;
					controller.AddToTile( tile );
					SelectedController = controller;
					SelectedController?.SetVisible( true );
				}
			}

			DeleteWorldUi( tile );

			if ( SelectedController?.GetVisible() ?? false )
			{
				foreach ( var neighbourTile in tile.GetNeighbours<GenericTile>() )
				{
					if ( neighbourTile != null )
					{
						int score = tile.GetTileScore( neighbourTile, SelectedController.GetTileType() );
						if ( score != 0 )
						{

							if ( neighbourTile.GetTileType() != TileTypeEnum.Base )
							{
								neighbourTile.SpawnUI();
								neighbourTile.UpdateWorldUI( Enum.GetName( typeof( GenericTile.TileTypeEnum ), neighbourTile.GetTileType() ), score );
							}
						}
					}
				}
			}

			SetGhost( tile );


			if ( this?.SelectedController?.GetVisible() ?? false )
			{
				Ghost.RenderColor = Color.Green;
			} else
			{
				Ghost.RenderColor = Color.Red;
			}
		}

		[ClientRpc]
		public void SetGhost( GenericTile tile )
		{
			if ( IsClient )
			{
				if ( tile != null )
				{

					if ( Ghost == null )
					{
						Ghost = Create<ModelEntity>();
					}

					Ghost.SetModel( "models/roads/street_ghost.vmdl" );
					Ghost.Position = tile.Position;
					Ghost.Scale = tile.Scale;
					Ghost.EnableShadowCasting = false;

				} else
				{
					Ghost?.Delete();
					Ghost = null;
				}

			}
		}

		public void ClientSimulate()
		{
			// Gameplay Controls
			if ( MyGame.GameState == MyGame.GameStateEnum.Playing )
			{
				if ( IsClient )
				{
					// Always raycast to check if the user has moved their selection else-where.
					GenericTile tile = GetTileLookedAt();

					if ( Input.Pressed( InputButton.Attack1 ) )
					{
						var tileData = SelectedController?.SerializeToJson();
						PlaceTile( tileData );
					}

					// Update when we have moved our Cursor onto another Tile.
					// This is to medigate the amount of updates being done to a tile, unless it is necessary.
					if ( LastHighlighted != tile )
					{
						if ( LastHighlighted != null )
						{
							OnTileHoverOff( LastHighlighted );
							LastHighlighted = null;
						}

						if ( tile != null )
						{
							OnTileHover( tile );
							LastHighlighted = tile;
						}

						LastSelectedTile = tile;
						LastSelectedTileType = SelectedTileType;
					}
				}

			}
		}

	}
}
