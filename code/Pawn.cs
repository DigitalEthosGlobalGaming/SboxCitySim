using Degg.Util;
using Sandbox;
using System;
using static CitySim.GenericTile;

namespace CitySim
{
	partial class Pawn : AnimatedEntity
	{
		/// <summary>
		/// Called when the entity is first created 
		/// </summary>
		/// 

		public static Pawn GetClientPawn()
		{

			return (Pawn)Game.LocalClient?.Pawn;
		}

		public int Score
		{
			get { return Client.GetInt( "score", 0 ); }
			set
			{
				try
				{
					Client.SetInt( "score", value );
				}
				catch ( Exception e )
				{
					AdvLog.Error( e );
				}
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

		private const float CameraFollowSmoothing = 100.0f;

		public TileController ClientController { get; set; }

		public ModelEntity Ghost { get; set; }

		public float UserRotation { get; set; } = 0f;

		[Net, Predicted]
		private float UpDistance { get; set; }

		// An example BuildInput method within a player's Pawn class.
		[ClientInput] public Vector3 InputDirection { get; protected set; }
		[ClientInput] public Angles ViewAngles { get; set; }

		public override void BuildInput()
		{
			InputDirection = Input.AnalogMove;

			var look = Input.AnalogLook;

			var viewAngles = ViewAngles;
			viewAngles += look;
			ViewAngles = viewAngles.Normal;
		}
		public override void Spawn()
		{
			base.Spawn();
			UpDistance = 250f;


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


		[ConCmd.Server]
		public static void SetControlsDisabledCmd( bool value )
		{
			Pawn pawn = (Pawn)ConsoleSystem.Caller.Pawn;
			pawn.DisabledControls = value;
		}


		public void ResetPivotPoint()
		{
			PivotPoint = (MyGame.GameObject.Map?.Position ?? null).GetValueOrDefault( Vector3.Zero );
		}

		public GenericTile GetTileLookedAt()
		{
			var endPos = Position + (Rotation.Forward * 4000);
			var mytrace = Trace.Ray( Position, endPos );

			if ( GhostTile != null )
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
		public override void Simulate( IClient cl )
		{
			if ( DisabledControls && false )
			{
				return;
			}

			if ( AdvInput.Pressed( InputButton.Reload ) )
			{
				ResetPivotPoint();
			}

			// Handle the Controls
			if ( Input.UsingController )
			{
				UpDistance += ViewAngles.ToRotation().y;
			}
			else
			{
				UpDistance -= (Input.MouseWheel * 10);
			}

			if ( AdvInput.Pressed( InputButton.Menu, InputButton.SlotNext ) )
			{
				UserRotation = UserRotation + 45;
			}
			// E
			else if ( AdvInput.Pressed( InputButton.Use, InputButton.SlotPrev ) )
			{
				UserRotation = UserRotation - 45;
			}

			UpDistance = MathX.Clamp( UpDistance, 50, 250 );

			var rot = Rotation.FromAxis( Vector3.Up, UserRotation );
			var direction = rot.Forward;
			var position = PivotPoint + (direction * (200f - (UpDistance * 0.35f)));
			position = position + (Vector3.Up * (UpDistance * 1f));


			// Handle the Camera looking at the pivot point
			Rotation = Rotation.LookAt( (PivotPoint - Position).Normal );

			// Ensure we have a 2D Rotation Forward
			Vector3 flatForward = Rotation.Forward;
			flatForward.z = 0;
			flatForward = flatForward.Normal;

			// build movement from the input values
			var movement = InputDirection.Normal;

			// move it towards the direction we're facing
			Velocity = Rotation.LookAt( flatForward ) * movement;

			// apply some speed to it
			Velocity *= Input.Down( InputButton.Run ) ? 400 : 200;

			// Modify the pivot point position.
			PivotPoint += Velocity * Time.Delta;
			if ( PivotPoint.z < MyGame.GetMap()?.Position.z )
			{
				ResetPivotPoint();
			}

			// Offset the Camera from the Pivot Point.
			Vector3 desiredPosition = position;

			// Animate towards the desired position.
			Position = Vector3.Lerp( Position, desiredPosition, Time.Delta * CameraFollowSmoothing );

			Camera.Position = Position;
			Camera.Rotation = Rotation;

			if ( Game.IsClient )
			{
				ClientSimulate();
			}
			else
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
			if ( tile != null )
			{
				foreach ( var nextTile in tile.GetNeighbours<GenericTile>() )
				{
					if ( nextTile != null )
					{
						nextTile.DestroyWorldUI();
					}
				}
			}
		}



		public void OnTileHover( GenericTile tile )
		{

			SetGhost( tile );
			if ( Game.IsClient )
			{
				RefreshSelectedTileType( tile );
			}
		}

		public void SelectNextRandomTile()
		{
			var start = 1;
			var end = 5;
			var rndInt = Game.Random.Int( start, end );
			if ( rndInt > 4 )
			{
				SelectNextTile( GenericTile.TileTypeEnum.House );
			}
			else
			{
				SelectNextTile( (GenericTile.TileTypeEnum)Enum.GetValues( typeof( GenericTile.TileTypeEnum ) ).GetValue( rndInt ) );
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
		public void SpawnGhost( GenericTile tile )
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
