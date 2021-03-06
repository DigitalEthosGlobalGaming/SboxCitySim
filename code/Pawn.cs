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

		public RoadTile LastHighlighted { get; set; }

		[Net, Local]
		public bool DisabledControls { get; set; }

		[Net, Local]
		public RoadTile StartSpace { get; set; }
		public RoadTile EndSpace { get; set; }
		public List<GridSpace> OldPath { get; set; } = new List<GridSpace>();

		public bool IsAdmin { get; set; } = false;

		[Net]
		public RoadTile.TileTypeEnum SelectedTileType { get; set; } = RoadTile.TileTypeEnum.Base;
		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );

			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
		}
		
		[Event.Hotload]
		public void OnLoad()
		{

		}


		[ServerCmd]
		public static void SetControlsDisabledCmd(bool value)
		{
			Pawn pawn = (Pawn)ConsoleSystem.Caller.Pawn;
			pawn.DisabledControls = value;
			Log.Info( pawn );
		}

		public RoadTile GetRoadTileLookedAt()
		{
			var endPos = EyePosition + (EyeRotation.Forward * 4000);
			var mytrace = Trace.Ray( EyePosition, endPos );
			var tr = mytrace.Run();
			if ( tr.Entity != null && tr.Entity is RoadTile )
			{
				RoadTile ent = (RoadTile)tr.Entity;
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

				// If we're running serverside and Attack1 was just pressed, spawn a ragdoll
				if ( IsServer)
				{
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
					} else if ( Input.Pressed( InputButton.Attack2 ) )
					{
						SelectedTileType = RoadTile.TileTypeEnum.Base;
					}


				}

				if ( IsServer && IsAdmin )
				{
					if ( Input.Pressed( InputButton.Slot0 ) )
					{
						Log.Info( "1" );
						SelectedTileType = RoadTile.TileTypeEnum.Base;
					}
					if ( Input.Pressed( InputButton.Slot1 ) )
					{
						Log.Info( "2" );
						SelectedTileType = RoadTile.TileTypeEnum.Business;
					}
					if ( Input.Pressed( InputButton.Slot2 ) )
					{
						Log.Info( "3" );
						SelectedTileType = RoadTile.TileTypeEnum.Park;
					}
					if ( Input.Pressed( InputButton.Slot3 ) )
					{
						Log.Info( "4" );
						SelectedTileType = RoadTile.TileTypeEnum.House;
					}

					if ( Input.Pressed( InputButton.Slot4 ) )
					{
						Log.Info( "5" );
						SelectedTileType = RoadTile.TileTypeEnum.Road;
					}
				}


				if ( IsClient )
				{

					var tile = GetRoadTileLookedAt();

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
				}
	
			}
		}

		public bool CanPlaceTyle(RoadTile tile)
		{
			if (IsAdmin)
			{
				return true;
			}

			if (tile.HasRoad() )
			{
				return false;
			}

			if (tile.TileType == RoadTile.TileTypeEnum.Base)
			{
				return true;
			}
			return false;
		}

		public void OnTileHoverOff(RoadTile tile)
		{
			try
			{
				tile.UpdateModel();
			} catch(Exception e)
			{

			}
		}

		public void OnTileHover(RoadTile tile)
		{

			tile.UpdateModel();
			if ( IsClient )
			{
				if (!CanPlaceTyle(tile))
				{
					return;
				}

				if (tile.CanSetType(SelectedTileType)) {
					tile.RenderColor = Color.Gray;
				} else
				{
					tile.RenderColor = Color.Yellow;
				}
			}
		}

		public void OnTileSelect(RoadTile tile)
		{
			var score = tile.SetTileType( SelectedTileType );
			SelectedTileType = RoadTile.TileTypeEnum.Base;
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
