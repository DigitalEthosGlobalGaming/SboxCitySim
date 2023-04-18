using CitySim.Utils;
using Degg.GridSystem;
using Sandbox;
using System;
using System.Collections.Generic;

namespace CitySim
{
	public partial class MovementEntity : ModelEntity, ITickable
	{

		public GenericTile LastHighlighted { get; set; }
		public List<GridSpace> NavPath { get; set; } = new List<GridSpace>();

		public int CurrentIndex = 0;
		public bool Moving = false;
		public float MovementSpeed { get; set; }

		public float MovementPercentage { get; set; }
		public float MaxMovementSpeed { get; set; }

		public float MovementAcceleration { get; set; }
		public Vector3 TargetPosition { get; set; }
		public GridSpace TargetSpace { get; set; }
		public GridSpace NextTargetSpace { get; set; }
		public Vector3 StartPosition { get; set; }

		public Queue<Func<bool>> OnFinishEvents = new Queue<Func<bool>>();
		public TickableCollection ParentCollection { get; set; }

		public Rotation TargetRotation {get;set;}

		public GridSpace NextTurnGridSpace { get; set; }

		public bool IsEmergencyVehicle { get; set; }
		public bool ShouldPullOver { get; set; } = true;
		public bool NearbyEmergencyVehicle { get; set; }

		public bool IsReverse { get; set; }
		public bool ShouldReverse { get; set; }

		public float CarHeight { get; set; } = 3f;

		public bool IsLastSpot { get; set; } = false;

		private Sound VehicleRevSound;

		public override void Spawn()
		{
			base.Spawn();
			VehicleRevSound = PlaySound( "vehicle.rev" );
			SetModel( "models/cars/normalcar1.vmdl" );
			SetBodyGroup( "base", Game.Random.Int( 0, 4 ) );
			TickableCollection.Global.Add( this );
			this.Name = "Movement Entity";
		}

		public void Init(List<GridSpace> path, bool shouldReverse = false )
		{
			ShouldReverse = shouldReverse;
			IsReverse = false;
			CurrentIndex = -1;
			Scale = 0.025f;
			MovementAcceleration = Game.Random.Float( 0.5f, 1f );
			MaxMovementSpeed = Game.Random.Float( 5f, 8f );
			NavPath = path;
			MoveToNext();
			Position = TargetPosition;
			Moving = true;

		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			OnFinish();

			VehicleRevSound.Stop();

			if ( ParentCollection != null )
			{
				ParentCollection.Remove( this );
			}
		}

		public void OnFinish()
		{
			while (OnFinishEvents.Count > 0)
			{
				var callback = OnFinishEvents.Dequeue();
				callback();
			}
		}

		public void OnClientTick( float delta, float currentTick )
		{
			
		}

		public void OnSharedTick( float delta, float currentTick )
		{

		}

		public void OnServerTick( float delta, float currentTick )
		{
			if ( Moving )
			{
				if ( ShouldPullOver )
				{
					// Player cause Cars to pull over for now.
					bool foundEmergency = false;

					foreach ( var entity in Entity.All )
					{
						if ( entity is MovementEntity vehicle )
						{
							if ( vehicle.IsEmergencyVehicle )
							{
								var eDist = entity.Position.Distance( TargetPosition );
								if ( eDist < 50.0f )
								{
									foundEmergency = true;
								}
								break;
							}
						}
					}
					NearbyEmergencyVehicle = foundEmergency;
				}
				
				MaxMovementSpeed = 10f;

				if ( MovementPercentage > 1f )
				{
					MovementPercentage = 0;

						var canMoveToNext = MoveToNext();

						if ( !canMoveToNext )
						{
							if ( ShouldReverse )
							{
								if ( IsReverse )
								{
									Delete();
									return;
								}
								else
								{
									TargetPosition = Position;
									StartPosition = Position;
									IsReverse = true;
									CurrentIndex = NavPath.Count - 1;

									//DebugOverlay.Line( Position, Position + (Vector3.Up * 10.0f), Color.Red, 2.0f, false );
									return;
								}
							}
							else
							{
								Delete();
								return;
							}
						}
				}

				var accelleration = MovementAcceleration;

				var newRotation = Rotation.LookAt( TargetPosition - Position, Vector3.Up);
				var turningDistance = Rotation.Distance( newRotation );
				var isTurning = turningDistance > 10;

				Rotation = newRotation;

				if ( MovementSpeed > MaxMovementSpeed )
				{
					MovementSpeed = MaxMovementSpeed;
				}
				else
				{
					if ( isTurning )
					{
						// Deccelerate
						MovementSpeed /= 3;
					}
					else
					{
						// Accellerate
						MovementSpeed += (accelleration * delta);
					}


					if ( NearbyEmergencyVehicle )
					{
						// Slow way down!
						MovementSpeed *= 0.05f;
					}
				}

				MovementPercentage += (MovementSpeed * delta);

				var position = ((TargetPosition - StartPosition) * MovementPercentage) + StartPosition;
				Position = position;
				if ( IsLastSpot )
				{
					RenderColor = RenderColor.WithAlpha( 1 - MovementPercentage );
				} else
				{
					RenderColor = RenderColor.WithAlpha( 1 );
				}
			}
		}


		public Vector3 NextPathPosition(float _rightOffset = 1.0f)
		{
			var offset = Game.Random.Float( _rightOffset * 0.5f, _rightOffset );
			var nextPosition = TargetSpace.Position + (Vector3.Up * CarHeight);
			Rotation perdictedRotation = Rotation.LookAt( nextPosition - Position, Vector3.Up );
			nextPosition += (perdictedRotation.Right * offset);
			return nextPosition;
		}

		public bool MoveToNext()
		{
			if ( IsReverse )
			{
				CurrentIndex = CurrentIndex - 1;
			} else
			{
				CurrentIndex = CurrentIndex + 1;
			}

			IsLastSpot = false;
			if (ShouldReverse)
			{
				if ( CurrentIndex <= 0 )
				{
					IsLastSpot = true;
				}
			} else
			{
				if ( CurrentIndex >= NavPath.Count )
				{
					IsLastSpot = true;
				}
			}

			if ( CurrentIndex >= 0 && CurrentIndex < NavPath.Count )
			{
				var nextSpace = NavPath[CurrentIndex];
				TargetSpace = nextSpace;

				StartPosition = TargetPosition;

				// Get the Next Path Position, with the offset.	
				TargetPosition = NextPathPosition( (IsEmergencyVehicle) ? 1.0f : 6.0f * (NearbyEmergencyVehicle ? 2.0f : 1.0f) );
			
				return true;
			} 
			else
			{
				return false;
			}
		}

	}
}
