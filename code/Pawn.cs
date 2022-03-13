using System;
using System.Collections.Generic;
using Degg.Analytics;
using Degg.GridSystem;
using Sandbox;
using static CitySim.GenericTile;

namespace CitySim
{
	partial class Pawn : AnimEntity
	{
		/// <summary>
		/// Called when the entity is first created 
		/// </summary>
		/// 

		public static Pawn GetClientPawn()
		{
			
			return (Pawn)Local.Client?.Pawn;
		}

		public int Score {
			get { return Client.GetInt("score", 0); }
			set
			{
				Client.SetInt( "score", value );
			}
		}

		public GenericTile LastHighlighted { get; set; }

		public bool HasReadWelcome { get; set; }


		[Net, Local]
		public bool DisabledControls { get; set; }

		// wtf does this do?
		/*[Net, Local]
		public RoadTile StartSpace { get; set; }
		public RoadTile EndSpace { get; set; }
		*/
		public GenericTile LastSelectedTile { get; set; }
		public List<GridSpace> OldPath { get; set; } = new List<GridSpace>();

		public ModelEntity GhostTile { get; private set; }

		[Net]
		public GenericTile.TileTypeEnum SelectedTileType { get; set; } = GenericTile.TileTypeEnum.Base;
		public GenericTile.TileTypeEnum LastSelectedTileType { get; set; } = GenericTile.TileTypeEnum.Base;
		[Net]
		public int TileBodyIndex { get; set; } = 0;

		[Net]
		public int TileMaterialIndex { get; set; } = 0;

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
		}

		public GenericTile GetRoadTileLookedAt()
		{
			var endPos = EyePosition + (EyeRotation.Forward * 4000);
			var mytrace = Trace.Ray( EyePosition, endPos );
			if (GhostTile != null)
			{
				mytrace.Ignore( GhostTile );
			}
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
							if (  tile.CanSetType( SelectedTileType ) && CanPlaceTyle( tile ) )
							{
								PlaceOnTile( tile );
								GameAnalytics.TriggerEvent(Client.PlayerId.ToString(), "tile_place", (int) tile.TileType);
							}
							else
							{
								PlaySoundClientSide( "ui.navigate.deny" );
								GameAnalytics.TriggerEvent( Client.PlayerId.ToString(), "tile_place", -1 );
							}
						}
					}
					// Right Click or LT
					else if ( Input.Pressed( InputButton.Attack2 ) )
					{

						if ( SelectedTileType != TileTypeEnum.Base )
						{
							PlaySoundClientSide( "ui.navigate.back" );
							GameAnalytics.TriggerEvent( Client.PlayerId.ToString(), "tile_place", -2 );
						}

						SelectedTileType = TileTypeEnum.Base;
					}

#if DEBUG && !RELEASE
					if ( MyGame.CurrentGameOptions.Mode == MyGame.GameModes.Sandbox )
					{
						GenericTile.TileTypeEnum? tileToSelect = null;
						// Debug controls for developers to test tiles.
						if ( Input.Pressed( InputButton.Slot0 ) )
						{
							tileToSelect = GenericTile.TileTypeEnum.Base;
						}
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
						if (tileToSelect != null)
						{
							SelectNextTile( tileToSelect.Value );
						}
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
			if (tile != null && !tile.IsValid)
			{
				LastHighlighted = null;
				return;
			}

			DestroyGhost( tile );
			tile.UpdateModel();
			tile.DestroyWorldUI();
			foreach (var nextTile in tile.GetNeighbours<GenericTile>())
			{
				if (nextTile != null)
				{
					nextTile.DestroyWorldUI();
				}
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

				SpawnGhost( tile );

				if ( tile.CanSetType( SelectedTileType ) )
				{
					tile.RenderColor = Color.Green;
					
					foreach ( var neighbourTile in tile.GetNeighbours<GenericTile>() )
					{
						if (neighbourTile != null)
						{
							int score = tile.GetTileScore( neighbourTile, SelectedTileType );

							if ( neighbourTile.TileType != TileTypeEnum.Base )
							{
								neighbourTile.SpawnUI();
							
								neighbourTile.UpdateWorldUI( Enum.GetName( typeof( GenericTile.TileTypeEnum ), neighbourTile.TileType ), score );
							}
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

		public void SelectNextRandomTile()
		{
			var start = 1;
			var end = 5;
			var rndInt = Rand.Int( start, end );
			if ( rndInt > 4 )
			{
				SelectNextTile( GenericTile.TileTypeEnum.House );
			}
			else
			{
				SelectNextTile((GenericTile.TileTypeEnum)Enum.GetValues( typeof( GenericTile.TileTypeEnum ) ).GetValue( rndInt ));
			}
		}

		public void SelectNextTile( TileTypeEnum type, int? nextBodyIndex = null, int? nextMaterialIndex = null )
		{
			switch ( type )
			{
				case TileTypeEnum.Business:
					TileBodyIndex = nextBodyIndex % 2 ?? Rand.Int( 0, 2 );
					TileMaterialIndex = nextMaterialIndex % 6 ?? Rand.Int( 0, 6 );
					break;
				case TileTypeEnum.House:
					TileBodyIndex = nextBodyIndex % 4 ?? Rand.Int( 0, 4 );
					TileMaterialIndex = nextMaterialIndex % 6 ?? Rand.Int( 0, 6 );
					break;
				default:
					TileBodyIndex = 0;
					TileMaterialIndex = 0;
					break;
			}
			SelectedTileType = type;
		}

		public void PlaceOnTile( GenericTile tile )
		{
			var score = tile.SetTileType( SelectedTileType, TileBodyIndex, TileMaterialIndex );
			if ( MyGame.CurrentGameOptions.Mode != MyGame.GameModes.Sandbox )
			{
				SelectedTileType = GenericTile.TileTypeEnum.Base;
			} else
			{
				SelectNextTile( SelectedTileType );
			}
			var clientScore = Client.GetInt( "score", 0 );
			clientScore = clientScore + score;
			Client.SetInt( "score", clientScore );

			// Place!
			PlaySoundClientSide( "physics.wood.impact" );
		}


		/// <summary>
		/// Called every frame on the client
		/// </summary>
		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );


		}

		[ClientRpc]
		public void PlaySoundClientSide( string name )
		{
			var player = GetClientPawn();
			player.PlaySound( name );
		}


		[ClientRpc]
		public void SpawnGhost(GenericTile tile)
		{
			if ( tile != null )
			{
				tile.EnableDrawing = false;

				if ( SelectedTileType != GenericTile.TileTypeEnum.Base && tile.CanSetType( SelectedTileType ) )
				{
					if ( GhostTile == null )
					{
						// Spawn's the Ghost Local Model
						GhostTile = new ModelEntity();
						GhostTile.EnableDrawOverWorld = true;
						GhostTile.Name = "Ghost Tile";
						GhostTile.PhysicsEnabled = false;
						GhostTile.Transmit = TransmitType.Never;
						GhostTile.EnableHitboxes = false;
						GhostTile.EnableAllCollisions = false;
						GhostTile.EnableSelfCollisions = false;
						GhostTile.EnableSolidCollisions = false;
						GhostTile.EnableTouch = false;
					}
					else
					{
						// Ghost Local Model exists, just update and draw the object.
						GhostTile.EnableDrawing = true;
					}

					GhostTile.Transform = tile.Transform;
					GenericTile.UpdateModel( GhostTile, SelectedTileType, TileBodyIndex, TileMaterialIndex );
					GenericTile.CheckModel( tile, GhostTile, SelectedTileType );
					GhostTile.RenderColor = new Color( 0, 1, 0, 0.75f );
				}
				else
				{
					DestroyGhost( tile );
				}
			}


		}
		[ClientRpc]
		public void DestroyGhost( GenericTile tile )
		{
			if ( tile != null && tile.IsValid )
			{
				tile.EnableDrawing = true;
			}

			if ( GhostTile != null )
			{
				GhostTile.EnableDrawing = false;
			}
		}
	}
}
