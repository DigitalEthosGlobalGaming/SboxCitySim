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

		public TileController SelectedController { get; set; }

		public ModelEntity GhostTile { get; private set; }

		[Net]
		public GenericTile.TileTypeEnum SelectedTileType { get; set; } = GenericTile.TileTypeEnum.Base;
		public GenericTile.TileTypeEnum LastSelectedTileType { get; set; } = GenericTile.TileTypeEnum.Base;
		[Net]
		public IDictionary<string, int> NextTileBodyGroups { get; set; }

		[Net]
		public int TileMaterialIndex { get; set; } = 0;

		[Net]
		public Vector3 PivotPoint { get; set; }

		private const float CameraFollowSmoothing = 30.0f;

		private const float distanceSensitivity = 5.0f;
		private float upDistance = 0.0f;
		private float forwardDistance = 0.0f;

		private const float rotationSensitivity = 0.15f;
		private float yaw = 0.0f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );

			EnableAllCollisions = false; // Disable all the collisions, so we can ensure we don't collide with the world.
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			EnableShadowCasting = false;
			EnableShadowInFirstPerson = false;

			// We need to make sure there is some data inside the for the next tile.
			SelectNextRandomTile();

			// We need to ensure we move the camera pivot point at the same height/location that the map is located at.
			ResetPivotPoint();
		}

		[Event.Hotload]
		public void OnLoad()
		{
			ResetPivotPoint();
		}


		[ServerCmd]
		public static void SetControlsDisabledCmd( bool value )
		{
			Pawn pawn = (Pawn)ConsoleSystem.Caller.Pawn;
			pawn.DisabledControls = value;
		}


		public void ResetPivotPoint()
		{
			Log.Info( "Reset Pivot" );
			PivotPoint = MyGame.GameObject.Map.Position;
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
			if ( DisabledControls && false )
			{
				return;
			}

			// Handle the Controls
			if (Input.UsingController)
			{
				yaw += Input.Rotation.x;
				upDistance += Input.Rotation.y;
				forwardDistance -= Input.Rotation.y;
			}
			else
			{
				// Q
				if (Input.Down(InputButton.Menu))
				{
					yaw += 1 * rotationSensitivity;
				}
				// E
				else if (Input.Down(InputButton.Use))
				{
					yaw -= 1 * rotationSensitivity;
				}

				upDistance += Input.MouseWheel;
				forwardDistance -= Input.MouseWheel;
			}

			upDistance = MathX.Clamp( upDistance, 0, 50 );
			forwardDistance = MathX.Clamp( forwardDistance, 0, 10 );

			// Handle the Camera looking at the pivot point
			Rotation = Rotation.LookAt( (PivotPoint - Position).Normal );
			EyeRotation = Rotation;

			// Ensure we have a 2D Rotation Forward
			Vector3 flatForward = Rotation.Forward;
			flatForward.z = 0;
			flatForward = flatForward.Normal;

			// build movement from the input values
			var movement = new Vector3( Input.Forward, Input.Left, 0 ).Normal;

			// move it towards the direction we're facing
			Velocity = Rotation.LookAt( flatForward ) * movement;

			// apply some speed to it
			Velocity *= Input.Down( InputButton.Run ) ? 400 : 200;

			// Modify the pivot point position.
			PivotPoint += Velocity * Time.Delta;
			if (PivotPoint.z < MyGame.GetMap()?.Position.z)
			{
				ResetPivotPoint();
			}

			// Offset the Camera from the Pivot Point.
			Vector3 desiredPosition = PivotPoint																											// Place the Center
				+ (Vector3.Up * (100.0f + (upDistance * distanceSensitivity)))																				// Raise the Position up from the pivot
				+ ( ((Vector3.Right * MathF.Cos( yaw )) + (Vector3.Forward * MathF.Sin( yaw ))).Normal * (100.0f + (forwardDistance * 5.0f)) )				// Rotate and Push back the camera in the inverse rotation that the camera will be facing.
				;

			// Animate towards the desired position.
			Position = Vector3.Lerp( Position, desiredPosition, Time.Delta * CameraFollowSmoothing );

			// Gameplay Controls
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
							PlaceOnTile( tile );
							GameAnalytics.TriggerEvent( Client.PlayerId.ToString(), "tile_place", (int)tile.GetTileType()) ;
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
							PlaySoundClientSide( "ui.button.press" );
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

		public void OnTileHoverOff( GenericTile tile )
		{
			if (tile != null && !tile.IsValid)
			{
				LastHighlighted = null;
				return;
			}

			tile.RenderColor = Color.White;

			return;

			DestroyGhost( tile );
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

			tile.RenderColor = Color.Green;

			return;

			
			if ( IsClient )
			{
				tile.RenderColor = Color.Green;

				return; 


				SpawnGhost( tile );

				if ( tile.CanSetType( null ) )
				{
					tile.RenderColor = Color.Green;
					
					foreach ( var neighbourTile in tile.GetNeighbours<GenericTile>() )
					{
						if (neighbourTile != null)
						{
							int score = tile.GetTileScore( neighbourTile, SelectedTileType );

							if ( neighbourTile.GetTileType() != TileTypeEnum.Base )
							{
								neighbourTile.SpawnUI();
							
								neighbourTile.UpdateWorldUI( Enum.GetName( typeof( GenericTile.TileTypeEnum ), neighbourTile.GetTileType() ), score );
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
			NextTileBodyGroups = new Dictionary<string, int>();
			switch ( type )
			{
				case TileTypeEnum.Business:
					{
						NextTileBodyGroups.Add( "base", Rand.Int( 0, 2 ) );
						TileMaterialIndex = nextMaterialIndex % 6 ?? Rand.Int( 0, 6 );
						break;
					}
				case TileTypeEnum.House:
					{
						NextTileBodyGroups.Add( "base", Rand.Int( 0, 4 ) );
						TileMaterialIndex = nextMaterialIndex % 6 ?? Rand.Int( 0, 6 );
						break;
					}
				case TileTypeEnum.Park:
					{
						NextTileBodyGroups.Add( "rock1", Rand.Int( 0, 2 ) );
						NextTileBodyGroups.Add( "rock2", Rand.Int( 0, 2 ) );
						NextTileBodyGroups.Add( "rock3", Rand.Int( 0, 2 ) );
						NextTileBodyGroups.Add( "bush1", Rand.Int( 0, 2 ) );
						NextTileBodyGroups.Add( "bush2", Rand.Int( 0, 2 ) );
						NextTileBodyGroups.Add( "bush3", Rand.Int( 0, 2 ) );
						TileMaterialIndex = nextMaterialIndex % 6 ?? Rand.Int( 0, 6 );
					}
					break;
				default:
					{
						NextTileBodyGroups.Add( "base", 0 );
						TileMaterialIndex = 0;
						break;
					}
			}
			SelectedTileType = type;
		}

		public void PlaceOnTile( GenericTile tile )
		{
			TileController controller = null;
			switch ( SelectedTileType )
			{
				case TileTypeEnum.Base:
					controller = new TileController();
					break;
				case TileTypeEnum.Park:
					controller = new ParkTileController();
					break;
				case TileTypeEnum.Business:
					controller = new BusinessTileController();
					break;
				case TileTypeEnum.House:
					controller = new HouseTileController();
					break;
				case TileTypeEnum.Road:
					controller = new RoadTileController();
					break;
				default:
					break;
			}

			if ( controller.CanAddToTile(tile))
			{
				tile.AddController(controller);
				PlaySoundClientSide( "physics.wood.impact" );
			} else
			{
				PlaySoundClientSide( "ui.navigate.deny" );
			}



			return;
			if ( MyGame.CurrentGameOptions.Mode != MyGame.GameModes.Sandbox )
			{
				SelectedTileType = GenericTile.TileTypeEnum.Base;
			} 
			else
			{
				SelectNextTile( SelectedTileType );
			}

			var clientScore = Client.GetInt( "score", 0 );
			// clientScore = clientScore + score;
			Client.SetInt( "score", clientScore );

			// Place!

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

				if ( SelectedTileType != GenericTile.TileTypeEnum.Base && tile.CanSetType( null ) )
				{
					if ( GhostTile == null )
					{
						// Spawn's the Ghost Local Model
						GhostTile = new ModelEntity
						{
							EnableDrawOverWorld = true,
							Name = "Ghost Tile",
							PhysicsEnabled = false,
							Transmit = TransmitType.Never,
							EnableHitboxes = false,
							EnableAllCollisions = false,
							EnableSelfCollisions = false,
							EnableSolidCollisions = false,
							EnableTouch = false
						};
					}
					else
					{
						// Ghost Local Model exists, just update and draw the object.
						GhostTile.EnableDrawing = true;
					}

					GhostTile.Transform = tile.Transform;
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
