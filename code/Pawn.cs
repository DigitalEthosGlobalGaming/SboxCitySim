using System;
using System.Collections.Generic;
using System.Text.Json;
using Degg.Analytics;
using Degg.GridSystem;
using Degg.Util;
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

		public TileController GhostController { get; set; }

		public ModelEntity GhostTile { get; private set; }

		[Net]
		public GenericTile.TileTypeEnum SelectedTileType { get; set; } = GenericTile.TileTypeEnum.Base;
		public GenericTile.TileTypeEnum LastSelectedTileType { get; set; } = GenericTile.TileTypeEnum.Base;
		
		[Net]
		public int TileMaterialIndex { get; set; } = 0;

		[Net, Predicted]
		public Vector3 PivotPoint { get; set; }

		private const float CameraFollowSmoothing = 30.0f;

		public TileController ClientController { get; set; }

		public ModelEntity Ghost { get; set; }

		public float UserRotation { get; set; } = 0f;

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

			PivotPoint = (MyGame.GameObject.Map?.Position ?? null).GetValueOrDefault(Vector3.Zero);
		}

		public GenericTile GetTileLookedAt()
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
				if (Input.Pressed( InputButton.Menu))
				{
					UserRotation = UserRotation + 45;
				}
				// E
				else if (Input.Pressed(InputButton.Use))
				{
					UserRotation = UserRotation - 45;
				}

				upDistance -= Input.MouseWheel;
				forwardDistance += Input.MouseWheel;
			}

			upDistance = MathX.Clamp( upDistance, 10, 50 );
			forwardDistance = MathX.Clamp( forwardDistance, 0, 10 );

			var rot = Rotation.FromAxis( Vector3.Up, UserRotation );
			var direction = rot.Forward;
			var position = PivotPoint + (direction * (200f - (upDistance * 3.5f)));
			position = position + (Vector3.Up * (upDistance * 5));


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
			Vector3 desiredPosition = position;

			// Animate towards the desired position.
			Position = Vector3.Lerp( Position, desiredPosition, Time.Delta * CameraFollowSmoothing );

			if (IsClient)
			{
				ClientSimulate();
			} else
			{
				ServerSimulate( cl );
			}
		}

		public void OnTileHoverOff( GenericTile tile )
		{
			DeleteWorldUi( tile );
		}

		public void DeleteWorldUi( GenericTile tile )
		{
			foreach ( var nextTile in tile.GetNeighbours<GenericTile>() )
			{
				if ( nextTile != null )
				{
					nextTile.DestroyWorldUI();
				}
			}
		}
		
		

		public void OnTileHover( GenericTile tile )
		{

			SetGhost( tile );
			if (IsClient) { 
				RefreshSelectedTileType(tile);				
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

		public void SetTileController()
		{
			TileController controller = TileController.GetTileControllerForType( SelectedTileType );
			this.GhostController = controller;
		}

		public void SelectNextTile( TileTypeEnum type, int? nextBodyIndex = null, int? nextMaterialIndex = null )
		{
			SelectedTileType = type;
			RefreshSelectedTileType();
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
