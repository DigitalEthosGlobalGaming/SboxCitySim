using System.Collections.Generic;
using GridSystem;
using Sandbox;

namespace CitySim
{
	partial class Pawn : AnimEntity
	{
		/// <summary>
		/// Called when the entity is first created 
		/// </summary>
		/// 

		public RoadTile LastHighlighted { get; set; }

		public RoadTile StartSpace { get; set; }
		public RoadTile EndSpace { get; set; }
		public List<GridSpace> OldPath { get; set; } = new List<GridSpace>();
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

		public RoadTile GetRoadTileLookedAt()
		{
			var endPos = EyePosition + (EyeRotation.Forward * 4000);
			var mytrace = Trace.Ray( EyePosition, endPos );
			var tr = mytrace.Run();
			if ( tr.Entity != null )
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

			// If we're running serverside and Attack1 was just pressed, spawn a ragdoll
			if ( IsServer && Input.Pressed( InputButton.Attack1 ) )
			{
				var tile = GetRoadTileLookedAt();
				if (tile != null) {
					tile.RenderColor = Color.Orange;
					tile.SetHasRoad( !tile.HasRoad );
				}
			}

			if ( IsServer && Input.Pressed( InputButton.Attack2 )  )
			{
				var tile = GetRoadTileLookedAt();
				if ( tile != null )
				{
					if ( Input.Down( InputButton.Jump ) )
					{
						
						StartSpace = tile;
					} else
					{
						
							EndSpace = tile;
					}
					if (StartSpace != null && EndSpace != null)
					{

						var map = ((MyGame)Game.Current).Map;
						OldPath = map.CreatePath( StartSpace.GridPosition, EndSpace.GridPosition );
						foreach ( var item in OldPath )
						{
							item.Position = item.Position + Vector3.Up * 50f;
						}
						var ent = new MovementEntity(  );
						ent.Init( OldPath );

					}
				}
				
			}

			if ( IsClient )
			{
				var endPos = EyeRotation.Forward * 4000;
				var mytrace = Trace.Ray( EyePosition, EyePosition + endPos );
				var tr = mytrace.Run();
				if ( tr.Entity != null )
				{
					if ( LastHighlighted != null )
					{
						LastHighlighted.RenderColor = Color.White;
						LastHighlighted = null;
					}
					if ( tr.Entity is RoadTile )
					{
						RoadTile ent = (RoadTile)tr.Entity;
						if ( ent != null )
						{
							ent.RenderColor = Color.Gray;
							LastHighlighted = ent;
						}
					}
				}
			}
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
