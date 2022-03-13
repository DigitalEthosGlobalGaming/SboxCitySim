using CitySim.UI;
using CitySim.Utils;
using Degg.GridSystem;
using Sandbox;

namespace CitySim
{
	public partial class GenericTile : GridSpace, ITickable
	{
		public WorldTileStatUI WorldUI { get; private set; }
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
		public GenericTile UpTile { get; set; }
		[Net]
		public GenericTile DownTile { get; set; }
		[Net]
		public GenericTile LeftTile { get; set; }
		[Net]
		public GenericTile RightTile { get; set; }

		[Net]
		public float TransitionPercentage { get; set; } = 1f;

		public GenericTile[] Neighbours = { null, null, null, null };
		[Net]
		public int TotalConnected { get; set; }

		[Net]
		public Rotation TargetRotation { get; set; }
		public Vector3 TargetPosition { get; set; }
		[Net, Change]
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
			RoadType = (RoadTypeEnum.StreetEmpty);
			SetupPhysicsFromModel( PhysicsMotionType.Static, false );
			UpdateName();
		}

		[ClientRpc]
		public void SpawnUI()
		{
			// Prevent Double Spawn.
			if ( WorldUI != null )
				return;

			if ( TileType != TileTypeEnum.Base && TileType != TileTypeEnum.Road )
			{
				WorldUI = MyGame.Ui.CreateWorldUi();
			}
		}
		[ClientRpc]
		public void UpdateWorldUI( string _name, int _points = 0 )
		{
			if ( WorldUI == null )
				return;

			WorldUI.Name = _name;
			WorldUI.Points = _points;
		}
		[ClientRpc]
		public void DestroyWorldUI()
		{
			if ( WorldUI == null )
				return;

			WorldUI.Delete();
			WorldUI = null;
		}

		public void UpdateName()
		{
			var suffix = "";
			if (HasRoad())
			{
				suffix = "R";
			}

			Name = $"[{GridPosition.x},{GridPosition.y}] {suffix}";


			UpdateWorldUI( Name );
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
			if (a is GenericTile )
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
			SetTileType( TileTypeEnum.Road, 0, 0 );

			if ( hasRoad != HasRoad() )
			{
				var transitionAmount = 10f;
				Position = GetWorldPosition() + new Vector3( Rand.Float( -transitionAmount, transitionAmount ), Rand.Float( -transitionAmount, transitionAmount ), Rand.Float( 100f, 150f ) );
				SetTileType( TileTypeEnum.Road, 0, 0 );
				CheckModel();
				GenericTile[] neighbours = GetNeighbours<GenericTile>();
				foreach ( GenericTile neighbor in neighbours )
				{
					neighbor?.CheckModel();
				}
			}
			UpdateName();
		}

		public GenericTile GetRoadNeighbour()
		{
			GenericTile[] neighbours = GetNeighbours<GenericTile>();
			foreach (GenericTile tile in neighbours)
			{
				if (tile?.HasRoad() ?? false)
				{
					return tile;
				}
			}
			return null;
			
		}

		public void Test()
		{
			CheckModel();
		}

		public void CheckModel()
		{
			GenericTile.CheckModel( this );
		}
		public static void CheckModel( GenericTile influencer, ModelEntity ghostViewModel = null, TileTypeEnum? ghostTileType = null)
		{
			if ( ghostViewModel == null )
				ghostViewModel = influencer;

			float rotation = 0;


			GenericTile[] neighbours = influencer.GetNeighbours<GenericTile>();
			influencer.UpTile = neighbours[0];
			influencer.RightTile = neighbours[1];
			influencer.DownTile = neighbours[2];
			influencer.LeftTile = neighbours[3];
			influencer.Neighbours = neighbours;
			influencer.UpConnected = influencer.UpTile?.HasRoad() ?? false;
			influencer.RightConnected = influencer.RightTile?.HasRoad() ?? false;
			influencer.DownConnected = influencer.DownTile?.HasRoad() ?? false;
			influencer.LeftConnected = influencer.LeftTile?.HasRoad() ?? false;
			var up = influencer.UpConnected;
			var right = influencer.RightConnected;
			var down = influencer.DownConnected;
			var left = influencer.LeftConnected;

			if ( influencer.TileType == TileTypeEnum.Road || (ghostTileType != null && ghostTileType == TileTypeEnum.Road) )
			{
				// We are a road.

				RoadTypeEnum newRoadType = influencer.RoadType;
				int totalCount = 0;

				if ( up )
				{
					totalCount++;
				}
				if ( down )
				{
					totalCount++;
				}
				if ( left )
				{
					totalCount++;
				}
				if ( right )
				{
					totalCount++;
				}

				if ( totalCount == 3 ) // THREE WAYS
				{
					newRoadType = RoadTypeEnum.ThreeWay;
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
				else if ( totalCount == 2 ) // STRAIGHT OR CURVES
				{
					if ( left && right )
					{
						newRoadType = RoadTypeEnum.Straight;
						rotation = 90;
					}
					else if ( up && down )
					{
						newRoadType = RoadTypeEnum.Straight;
					}
					else
					{
						newRoadType = RoadTypeEnum.Curve;
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
				else if ( totalCount == 1 ) // DEAD ENDS
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
					newRoadType = RoadTypeEnum.DeadEnd;
				}
				else
				{
					newRoadType = RoadTypeEnum.FourWay;
				}

				// Update relating data.
				influencer.TotalConnected = totalCount;
				influencer.RoadType = newRoadType;

				// We want to override the base.
				ghostViewModel.SetBodyGroup( "base", (int)newRoadType );
			}
			else
			{
				// We are a building.

				if ( up )
				{
					rotation = 270;
				}
				else if ( down )
				{
					rotation = 90;
				}
				else if ( left )
				{
					rotation = 180;
				}
				else if ( right )
				{
					rotation = 0;
				}
			}

			influencer.TargetRotation = Rotation.FromAxis( Vector3.Up, rotation );
			influencer.TargetPosition = influencer.GetWorldPosition();
			influencer.TransitionPercentage = 0f;

			if ( ghostViewModel != null )
			{
				ghostViewModel.Position = influencer.GetWorldPosition();
				ghostViewModel.Rotation = Rotation.FromAxis( Vector3.Up, rotation );
			}

		}


		public override void Spawn()
		{
			base.Spawn();
			TickableCollection.Global.Add( this );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();
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
			if (WorldUI != null)
			{
				var position = GetWorldPosition();
				var distance = position.Distance( Pawn.GetClientPawn().EyePosition );
				if (distance == 0)
				{
					distance = 1;
				}
				float scale =  500 / distance;
				WorldUI.SetScale( scale);
				WorldUI.SetPosition( position + (Vector3.Up * 10f) );
			}
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
			} 
			else
			{
				TransitionPercentage = 1;
			}
		}


		public override string ToString()
		{
			UpdateName();
			return $"SPACE {Name}";
		}

	}


}
