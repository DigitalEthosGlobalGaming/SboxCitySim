using GridSystem;
using Sandbox;
using System;

namespace CitySim
{
	public partial class RoadTile : GridSpace, ITickable
	{
		public TickableCollection ParentCollection { get; set; }

		public enum RoadTypeEnum
		{
			Straight = 0,
			Curve = 1,
			ThreeWay = 2,
			FourWay = 4,
			DeadEnd = 11,
			StreetEmpty = 14,
			WaterEmpty = 15,
		}

		[Net]
		public bool UpConnected { get; set; } = false;
		[Net]
		public bool DownConnected { get; set; } = false;
		[Net]
		public bool LeftConnected { get; set; } = false;
		[Net]
		public bool RightConnected { get; set; } = false;
		[Net]
		public RoadTile UpTile { get; set; }
		[Net]
		public RoadTile DownTile { get; set; }
		[Net]
		public RoadTile LeftTile { get; set; }
		[Net]
		public RoadTile RightTile { get; set; }

		[Net]
		public float TransitionPercentage { get; set; } = 1f;

		public RoadTile[] Neighbours = { null, null, null, null };
		[Net]
		public int TotalConnected { get; set; }

		[Net]
		public Rotation TargetRotation {get;set;}
		public Vector3 TargetPosition { get; set; }
		[Net,Change]
		public RoadTypeEnum RoadType { get; set; }
		/// <summary>
		/// Called when the entity is first created 
		/// </summary>
		public override void OnAddToMap()
		{
			base.OnAddToMap();
			Position = GetWorldPosition();
			TargetPosition = Position;
			SetModel( "models/roads/street_4way.vmdl" );
			RoadType = ( RoadTypeEnum.StreetEmpty );
			SetupPhysicsFromModel( PhysicsMotionType.Static, false );
			UpdateName();
		}

		public void UpdateName()
		{
			var suffix = "";
			if (HasRoad())
			{
				suffix = "R";
			}

			Name = $"[{GridPosition.x},{GridPosition.y}] {suffix}";
		}

		public bool HasRoad()
		{
			return TileType == TileTypeEnum.Road;
		}

		public void OnRoadTypeChanged( RoadTypeEnum oldValue, RoadTypeEnum newValue )
		{
			SetBodyGroup( "base", (int)newValue );
		}


		public override float GetMovementWeight( GridSpace a )
		{
			if (a is RoadTile )
			{
				if ( HasRoad() )
				{
					return 10;
				}
			}
				
			return -1;
		}


		public void SetHasRoad(bool hasRoad)
		{
			SetTileType( TileTypeEnum.Road );

			if ( hasRoad != HasRoad() )
			{
				var transitionAmount = 10f;
				Position = GetWorldPosition() + new Vector3( Rand.Float( -transitionAmount, transitionAmount ), Rand.Float( -transitionAmount, transitionAmount ), Rand.Float( 100f, 150f ) );
				SetTileType( TileTypeEnum.Road );
				CheckModel();
				RoadTile[] neighbours = GetNeighbours<RoadTile>();
				neighbours[0]?.CheckModel();
				neighbours[1]?.CheckModel();
				neighbours[2]?.CheckModel();
				neighbours[3]?.CheckModel();
			}
			UpdateName();
		}

		public RoadTile GetRoadNeighbour()
		{
			RoadTile[] neighbours = GetNeighbours<RoadTile>();
			if (neighbours[0]?.HasRoad() ?? false)
			{
				return neighbours[0];
			}
			if ( neighbours[1]?.HasRoad() ?? false )
			{
				return neighbours[1];
			}
			if ( neighbours[2]?.HasRoad() ?? false )
			{
				return neighbours[2];
			}
			if ( neighbours[3]?.HasRoad() ?? false )
			{
				return neighbours[3];
			}
			return null;
			
		}

		public void Test()
		{
			CheckModel();
		}



		public void CheckModel()
		{
			var newRoadType = RoadType;
			var rotation = 0f;

			RenderColor = Color.White;
			if ( !HasRoad() )
			{
				RoadType = RoadTypeEnum.StreetEmpty;
			}
			else
			{

				RoadTile[] neighbours = GetNeighbours<RoadTile>();
				UpTile = neighbours[0];
				RightTile = neighbours[1];
				DownTile = neighbours[2];
				LeftTile = neighbours[3];
				Neighbours = neighbours;
				UpConnected = UpTile?.HasRoad() ?? false;
				RightConnected = RightTile?.HasRoad() ?? false;
				DownConnected = DownTile?.HasRoad() ?? false;
				LeftConnected = LeftTile?.HasRoad() ?? false;
				var up = UpConnected;
				var right = RightConnected;
				var down = DownConnected;
				var left = LeftConnected;
				int totalCount = 0;



				if ( up == true )
				{
					totalCount = totalCount + 1;
				}
				if ( down == true )
				{
					totalCount = totalCount + 1;
				}
				if ( left == true )
				{
					totalCount = totalCount + 1;
				}
				if ( right == true )
				{
					totalCount = totalCount + 1;
				}

				if ( totalCount == 3 )
				{
					if ( up == false )
					{
						rotation = 0;
					}
					else if ( !right )
					{
						rotation = 1;
					}
					else if ( !down )
					{
						rotation = 2;
					}
					else if ( !left )
					{
						rotation = 2;
					}
				}

				if ( totalCount == 4 )
				{
					RoadType = RoadTypeEnum.FourWay;
				}
				else if ( totalCount == 3 )
				{
					RoadType = RoadTypeEnum.ThreeWay;
					if ( !up )
					{
						rotation = 180;
					}
					else if ( !right )
					{
						rotation = 270;
					}
					else if ( !down )
					{
						rotation = 0;
					}
					else if ( !left )
					{
						rotation = 90;
					}
				}
				else if ( totalCount == 2 )
				{
					if ( left && right )
					{
						RoadType = RoadTypeEnum.Straight;
						rotation = 90;
					}
					else if ( up && down )
					{
						RoadType = RoadTypeEnum.Straight;
					}
					else
					{
						RoadType = RoadTypeEnum.Curve;
						if ( up )
						{
							if ( left )
							{
								rotation = 180;
							}
							else
							{
								rotation = 270;
							}
						}
						if ( down )
						{
							if ( left )
							{
								rotation = 90;
							}
							else
							{
								rotation = 0;
							}
						}


					}
				}
				else if ( totalCount == 1 )
				{
					if ( up )
					{
						rotation = 0;
					}
					else if ( right )
					{
						rotation = 90;
					}
					else if ( down )
					{
						rotation = 180;
					}
					else if ( left )
					{
						rotation = 270;
					}
					RoadType = RoadTypeEnum.DeadEnd;
				}
				else
				{
					RoadType = RoadTypeEnum.FourWay;
				}
				TotalConnected = totalCount;

			}

			if ( newRoadType != RoadType)
			{
				TargetRotation = Rotation.FromAxis( new Vector3( 0, 0, 1 ), rotation );
				TargetPosition = GetWorldPosition();
				TransitionPercentage = 0f;
			}

			
		}

		public override void Spawn()
		{
			base.Spawn();
			TickableCollection.Global.Add( this );
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

		public void OnSharedTick( float delta, float currentTick )
		{

		}

		public void OnServerTick( float delta, float currentTick )
		{
			var transitionAmount = 5f;
			Rotation = Rotation.Slerp( Rotation, TargetRotation, transitionAmount * 2 * delta );
			Position = Position.LerpTo( TargetPosition, transitionAmount * delta );
			if ( TransitionPercentage < 1)
			{
				TransitionPercentage = TransitionPercentage + (10f * delta);
			} else
			{
				TransitionPercentage = 1;
			}
			UpdateNeeds();
		}


		public override string ToString()
		{
			UpdateName();
			return $"SPACE {Name}";
		}

	}


}
