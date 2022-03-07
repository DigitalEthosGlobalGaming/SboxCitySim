using System;
using System.Collections.Generic;
using GridSystem;
using Sandbox;
using Sandbox.UI;

namespace CitySim
{
	partial class Pawn : AnimEntity
	{
		/// <summary>
		/// Called when the entity is first created 
		/// </summary>
		/// 

		public GenericTile LastHighlighted { get; set; }

		[Net, Local]
		public bool DisabledControls { get; set; }

		// wtf does this do?
		/*[Net, Local]
		public RoadTile StartSpace { get; set; }
		public RoadTile EndSpace { get; set; }
		*/
		public GenericTile LastSelectedTile { get; set; }
		public List<GridSpace> OldPath { get; set; } = new List<GridSpace>();

		[Net]
		public GenericTile.TileTypeEnum SelectedTileType { get; set; } = GenericTile.TileTypeEnum.Base;
		public GenericTile.TileTypeEnum LastSelectedTileType { get; set; } = GenericTile.TileTypeEnum.Base;
		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );

			EnableAllCollisions = false; // Disable all the collisions, so we can ensure we don't collide with the world.
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
		}

		[Event.Hotload]
		public void OnLoad()
		{

		}


		[ServerCmd]
		public static void SetControlsDisabledCmd( bool value )
		{
			Pawn pawn = (Pawn)ConsoleSystem.Caller.Pawn;
			pawn.DisabledControls = value;
			Log.Info( pawn );
		}

		public GenericTile GetRoadTileLookedAt()
		{
			var endPos = EyePosition + (EyeRotation.Forward * 4000);
			var mytrace = Trace.Ray( EyePosition, endPos );
			var tr = mytrace.Run();
			if ( tr.Entity != null && tr.Entity is GenericTile )
			{
				GenericTile ent = (GenericTile)tr.Entity;
				return ent;
			}
			return null;
		}

		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		public override void Simulate( Client cl )
		{
			if ( DisabledControls )
			{
				return;
			}

			base.Simulate( cl );



			Rotation = Input.Rotation;
			EyeRotation = Rotation;

			// build movement from the input values
			var movement = new Vector3( Input.Forward, Input.Left, 0 ).Normal;

			// rotate it to the direction we're facing
			Velocity = Rotation * movement;

			// apply some speed to it
			Velocity *= Input.Down( InputButton.Run ) ? 1000 : 200;

			var endPost = EyePosition + (EyeRotation.Forward * 4000);

			// apply it to our position using MoveHelper, which handles collision
			// detection and sliding across surfaces for us

			MoveHelper helper = new MoveHelper( Position, Velocity );


			helper.Trace = helper.Trace.Size( 16 );
			if ( helper.TryMove( Time.Delta ) > 0 )
			{
				Position = helper.Position;
			}

			if ( MyGame.GameState == MyGame.GameStateEnum.Playing )
			{

				// Input Actions 
				if ( IsServer )
				{
					// Left Click or RT
					if ( Input.Pressed( InputButton.Attack1 ) )
					{
						var tile = GetRoadTileLookedAt();
						if ( tile != null )
						{
							if ( CanPlaceTyle( tile ) )
							{
								OnTileSelect( tile );
							}
						}
					}
					// Right Click or LT
					else if ( Input.Pressed( InputButton.Attack2 ) )
					{
						SelectedTileType = GenericTile.TileTypeEnum.Base;
					}

#if DEBUG && !RELEASE
					// Debug controls for developers to test tiles.
					if ( Input.Pressed( InputButton.Slot0 ) )
					{
						Log.Info( "1" );
						SelectedTileType = GenericTile.TileTypeEnum.Base;
					}
					if ( Input.Pressed( InputButton.Slot1 ) )
					{
						Log.Info( "2" );
						SelectedTileType = GenericTile.TileTypeEnum.Business;
					}
					if ( Input.Pressed( InputButton.Slot2 ) )
					{
						Log.Info( "3" );
						SelectedTileType = GenericTile.TileTypeEnum.Park;
					}
					if ( Input.Pressed( InputButton.Slot3 ) )
					{
						Log.Info( "4" );
						SelectedTileType = GenericTile.TileTypeEnum.House;
					}

					if ( Input.Pressed( InputButton.Slot4 ) )
					{
						Log.Info( "5" );
						SelectedTileType = GenericTile.TileTypeEnum.Road;
					}
				}
#endif

				if ( IsClient )
				{
					// Always raycast to check if the user has moved their selection else-where.
					GenericTile tile = GetRoadTileLookedAt();

					// Update when we have moved our Cursor onto another Tile.
					// This is to medigate the amount of updates being done to a tile, unless it is necessary.
					if ( 
						LastSelectedTile != tile								|| 
						(LastSelectedTile != null && LastSelectedTile.IsDirty)	|| 
						LastSelectedTileType != SelectedTileType 
					)
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

		public bool CanPlaceTyle( GenericTile tile )
		{
			/*
			if (MyGame.InDevelopment)
			{
				return true;
			}
			*/

			if ( tile.HasRoad() )
			{
				return false;
			}

			if ( tile.TileType == GenericTile.TileTypeEnum.Base )
			{
				return true;
			}
			return false;
		}

		public void OnTileHoverOff( GenericTile tile )
		{
			try
			{
				tile.UpdateModel();
				tile.DestroyWorldUI();
				foreach (var nextTile in tile.GetNeighbours<GenericTile>())
				{
					nextTile.DestroyWorldUI();
				}
			}
			catch ( Exception e )
			{

			}
		}

		public void OnTileHover( GenericTile tile )
		{

			tile.UpdateModel();
			if ( IsClient )
			{
				if ( !CanPlaceTyle( tile ) )
				{
					return;
				}

				if ( tile.CanSetType( SelectedTileType ) )
				{
					tile.RenderColor = Color.Green;
					
					foreach ( var neighbourTile in tile.GetNeighbours<GenericTile>() )
					{
						if (neighbourTile != null)
						{
							int score = tile.GetTileScore( neighbourTile, SelectedTileType );
							
							neighbourTile.SpawnUI();
							neighbourTile.UpdateWorldUI( Enum.GetName( typeof( GenericTile.TileTypeEnum ), neighbourTile.TileType ), score);
						}
					}
				}
				else
				{
					tile.RenderColor = Color.Red;
					tile.SpawnUI();
					tile.UpdateWorldUI( "Unable to place object here." );

					foreach ( var nextTile in tile.GetNeighbours<GenericTile>() )
					{
						if ( nextTile != null )
						{
							nextTile.DestroyWorldUI();
						}
					}
				}
			}
		}

		public void OnTileSelect( GenericTile tile )
		{
			var score = tile.SetTileType( SelectedTileType );
			SelectedTileType = GenericTile.TileTypeEnum.Base;
			var clientScore = Client.GetInt( "score", 0 );
			clientScore = clientScore + score;
			Client.SetInt( "score", clientScore );
		}


		/// <summary>
		/// Called every frame on the client
		/// </summary>
		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );


		}
	}
}
