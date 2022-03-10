using GridSystem;
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

		public bool ShouldPullOver { get; set; }
		public bool NearbyEmergencyVehicle { get; set; }

		public bool IsReverse { get; set; }
		public bool ShouldReverse { get; set; }

		public float CarHeight { get; set; } = 5f;


		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/cars/normalcar1.vmdl" );
			SetBodyGroup( "base", Rand.Int( 0, 4 ) );
			TickableCollection.Global.Add( this );
		}

		public void Init(List<GridSpace> path, bool shouldReverse = false )
		{
			ShouldReverse = shouldReverse;
			IsReverse = false;
			CurrentIndex = -1;
			Scale = 0.05f;
			NavPath = path;
			MoveToNext();
			Position = TargetPosition;
			Moving = true;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			OnFinish();
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
						if ( entity is Pawn player )
						{
							var eDist = entity.Position.Distance( TargetPosition );
							if ( eDist < 150.0f )
							{
								//Log.Info( "Car detected police! " + eDist );
								foundEmergency = true;
							}
							break;
						}
					}
					NearbyEmergencyVehicle = foundEmergency;
				}
				
				MaxMovementSpeed = 10f;
				MovementAcceleration = 1f;

				if ( MovementPercentage > 1 )
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
					if ( NearbyEmergencyVehicle )
					{
						// Slow way down!
						MovementSpeed -= MovementSpeed * 0.15f;
					}

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
				}

				MovementPercentage += (MovementSpeed * delta);

				var position = ((TargetPosition - StartPosition) * MovementPercentage) + StartPosition;
				Position = position;
			}
		}


		public Vector3 NextPathPosition(float _rightOffset = 1.0f)
		{
			var nextPosition = TargetSpace.Position + (Vector3.Up * CarHeight);
			Rotation perdictedRotation = Rotation.LookAt( nextPosition - Position, Vector3.Up );
			nextPosition += (perdictedRotation.Right * _rightOffset);

			//DebugOverlay.Line( StartPosition, Transform.Position, Color.Magenta, 1, false );
			//DebugOverlay.Line( Transform.Position, nextPosition, Color.Blue, 1, false );
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

			if ( CurrentIndex >= 0 && CurrentIndex < NavPath.Count )
			{
				var nextSpace = NavPath[CurrentIndex];
				TargetSpace = nextSpace;

				StartPosition = TargetPosition;

				// Get the Next Path Position, with the offset.	
				TargetPosition = NextPathPosition( 6.0f * (NearbyEmergencyVehicle ? 2.0f : 1.0f) );
			
				return true;
			} 
			else
			{
				return false;
			}
		}

	}
}
