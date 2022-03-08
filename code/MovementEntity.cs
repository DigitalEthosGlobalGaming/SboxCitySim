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

		public bool IsReverse { get; set; }
		public bool ShouldReverse { get; set; }

		public float CarHeight { get; set; } = 10f;


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
						MovementSpeed = MovementSpeed / 2;
					}
					else
					{
						MovementSpeed = MovementSpeed + (accelleration * delta);
					}
				}

				MovementPercentage = MovementPercentage + (MovementSpeed * delta);

				var position = ((TargetPosition - StartPosition) * MovementPercentage) + StartPosition;
				Position = position;

			}
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
				TargetPosition = nextSpace.Position + (Vector3.Up * CarHeight);

				return true;
			} else
			{
				return false;
			}
		}

	}
}
