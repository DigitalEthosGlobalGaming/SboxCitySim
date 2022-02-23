using System.Collections.Generic;
using GridSystem;
using Sandbox;

namespace CitySim
{
	partial class MovementEntity : ModelEntity, ITickable
	{

		public RoadTile LastHighlighted { get; set; }
		public List<GridSpace> NavPath { get; set; } = new List<GridSpace>();

		public int CurrentIndex = 0;
		public bool Moving = false;
		public float MovementSpeed { get; set; }
		public Vector3 TargetPosition { get; set; }
		public TickableCollection ParentCollection { get; set; }


		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
			TickableCollection.Global.Add( this );
		}

		public void Init(List<GridSpace> path )
		{
			CurrentIndex = -1;
			NavPath = path;
			MoveToNext();
			Position = TargetPosition;
			Moving = true;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if ( ParentCollection != null )
			{
				ParentCollection.Remove( this );
			}
		}

		public void OnClientTick( float delta, float currentTick )
		{
			
		}

		public void OnServerTick( float delta, float currentTick )
		{
			if ( Moving )
			{
				MovementSpeed = 500f;
				var distance = TargetPosition - Position;
				Position = Position + (distance.Normal * MovementSpeed * delta);

				if ( Position.IsNearlyEqual( TargetPosition, (MovementSpeed / 100) ) )
				{
					var canMoveToNext = MoveToNext();

					if ( !canMoveToNext )
					{
						this.Delete();
					}
				}
			}
		}

		public bool MoveToNext()
		{
			CurrentIndex = CurrentIndex + 1;
			if ( CurrentIndex >= 0 && CurrentIndex < NavPath.Count )
			{
				var nextSpace = NavPath[CurrentIndex];
				TargetPosition = nextSpace.Position;
				return true;
			} else
			{
				return false;
			}
		}

	}
}
