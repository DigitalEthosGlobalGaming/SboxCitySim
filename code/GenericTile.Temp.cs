using CitySim.Utils;
using Degg.GridSystem;
using Sandbox;
using System.Collections.Generic;

namespace CitySim
{
	public partial class GenericTile : GridSpace, ITickable
	{
		public enum TileTypeEnum
		{
			Base = 0,
			Park = 1,
			Business = 2,
			House = 3,
			Road = 4,
		}

		[Net]
		public string CurrentModel { get; set; }

		public static string[] TileTypeModels;
		public static string[] RoadTypeModels;


		public static string GetModelForTileType( TileTypeEnum type, RoadTypeEnum roadType = RoadTypeEnum.StreetEmpty )
		{
			switch ( type )
			{
				case TileTypeEnum.Base:
					return "models/roads/street_empty.vmdl";
				case TileTypeEnum.Park:
					return "models/buildings/forest.vmdl";
				case TileTypeEnum.Business:
					return "models/buildings/shop.vmdl";
				case TileTypeEnum.House:
					return "models/buildings/house_01.vmdl";
				case TileTypeEnum.Road:
					{
						switch ( roadType )
						{
							case RoadTypeEnum.StreetEmpty:
								return "models/roads/street_empty.vmdl";
							case RoadTypeEnum.Straight:
								return "models/roads/street_straight.vmdl";
							case RoadTypeEnum.Curve:
								return "models/roads/street_curve.vmdl";
							case RoadTypeEnum.ThreeWay:
								return "models/roads/street_3way.vmdl";
							case RoadTypeEnum.FourWay:
								return "models/roads/street_4way.vmdl";
							case RoadTypeEnum.DeadEnd:
								return "models/roads/street_deadend.vmdl";
							case RoadTypeEnum.WaterEmpty:
								return "models/roads/street_water.vmdl";
							default:
								Log.Warning( $"No valid road type for {roadType}" );
								return "models/roads/street_empty.vmdl";
						}
					}
				default:
					return "models/roads/street_empty.vmdl";
			}
		}


		[Net]
		public TileTypeEnum TileType { get; set; }
		[Net]
		public IDictionary<string, int> BodyGroups { get; set; } = new Dictionary<string, int>();

		[Net]
		public int materialIndex { get; set; } = 0;

		public void UpdateModel()
		{
			GenericTile.UpdateModel( this, this.TileType, this.BodyGroups, this.materialIndex );
		}
		public void UpdateModel( IDictionary<string, int> BodyGroups = null, int? materialIndex = null )
		{
			if ( BodyGroups != null)
			{
				this.BodyGroups = BodyGroups;
			}

			if ( materialIndex  != null)
			{
				this.materialIndex = materialIndex.Value;
			}


			GenericTile.UpdateModel( this, TileType, BodyGroups, materialIndex ?? this.materialIndex );
		}

		public static void UpdateModel( ModelEntity entity, TileTypeEnum type, IDictionary<string, int> bodyGroups, int materialIndex )
		{
			var roadType = RoadTypeEnum.StreetEmpty;
			if (entity is GenericTile g)
			{
				roadType = g.RoadType;
			}

			var model = GetModelForTileType( type, roadType );
			entity.SetModel( model );
			entity.SetupPhysicsFromModel( PhysicsMotionType.Static, false );
			entity.SetMaterialGroup( materialIndex );

			if ( bodyGroups != null )
			{
				foreach ( var key in bodyGroups.Keys )
				{
					entity.SetBodyGroup( key, bodyGroups[key] );
				}
			}

			entity.RenderColor = Color.White;
		}

		public void SetModelBodyGroup(string key, int value)
		{
			if ( BodyGroups  == null)
			{
				BodyGroups = new Dictionary<string,int>();
			}

			BodyGroups[key] = value;
			UpdateModel(this.BodyGroups);
		}



		public void CheckNeighbours()
		{

			GenericTile[] neighbours = GetNeighbours<GenericTile>();
			neighbours[0]?.CheckModel();
			neighbours[1]?.CheckModel();
			neighbours[2]?.CheckModel();
			neighbours[3]?.CheckModel();
		}

		public static bool IsCombination( GenericTile aTile, GenericTile bTile, GenericTile.TileTypeEnum a, GenericTile.TileTypeEnum b, TileTypeEnum? pretenderForA = null )
		{
			GenericTile.TileTypeEnum aTileType = pretenderForA ?? aTile.TileType;
			if ( aTileType == a && bTile.TileType == b )
			{
				return true;
			}
			if ( bTile.TileType == a && aTileType == b )
			{
				return true;
			}

			return false;
		}

		public int GetTileScore( GenericTile otherTile, TileTypeEnum? pretender = null )
		{
			int score = 0;
			if ( otherTile != null )
			{
				// House House
				if ( IsCombination( this, otherTile, GenericTile.TileTypeEnum.House, GenericTile.TileTypeEnum.House, pretender ) )
				{
					score += 1;
				}
				// House Park
				else if ( IsCombination( this, otherTile, GenericTile.TileTypeEnum.House, GenericTile.TileTypeEnum.Park, pretender ) )
				{
					score += 2;
				}
				// House Business
				else if ( IsCombination( this, otherTile, GenericTile.TileTypeEnum.House, GenericTile.TileTypeEnum.Business, pretender ) )
				{
					score -= 1;
				}
				// Park Park
				else if ( IsCombination( this, otherTile, GenericTile.TileTypeEnum.Park, GenericTile.TileTypeEnum.Park, pretender ) )
				{
					score += 2;
				}
				// Park Road
				else if ( IsCombination( this, otherTile, GenericTile.TileTypeEnum.Park, GenericTile.TileTypeEnum.Road, pretender ) )
				{
					score -= 1;
				}
				// Park Business
				else if ( IsCombination( this, otherTile, GenericTile.TileTypeEnum.Park, GenericTile.TileTypeEnum.Business, pretender ) )
				{
					score -= 1;
				}
				// Business Business
				else if ( IsCombination( this, otherTile, GenericTile.TileTypeEnum.Business, GenericTile.TileTypeEnum.Business, pretender ) )
				{
					score += 2;
				}
			}
			return score;
		}

		public int SetTileType( TileTypeEnum type, IDictionary<string, int> bodyGroups, int materialIndex )
		{
			if (IsServer)
			{
				if (!CanSetType(type))
				{
					return 0;
				}

				TileType = type;

				var transitionAmount = 10f;
				Position = GetWorldPosition() + new Vector3( Rand.Float( -transitionAmount, transitionAmount ), Rand.Float( -transitionAmount, transitionAmount ), Rand.Float( 100f, 150f ) );


				SetupNeeds();

				
				if ( type == TileTypeEnum.Road )
				{
					CheckModel();
				}
				else
				{
					UpdateModel( bodyGroups, materialIndex );
				}
				CheckNeighbours();

				IsDirty = true;

				MyGame.GetMap().UpdateScore();
				return MyGame.GetMap().CalculateTileScore( this );
			}
			return 0;
		}

		public bool IsNextToRoad()
		{
			var neighbours = GetNeighbours<GenericTile>();
			if ( neighbours[0]?.HasRoad() ?? false )
			{
				return true;
			}
			if ( neighbours[1]?.HasRoad() ?? false )
			{
				return true;
			}
			if ( neighbours[2]?.HasRoad() ?? false )
			{
				return true;
			}
			if ( neighbours[3]?.HasRoad() ?? false )
			{
				return true;
			}
			return false;
		}

		public bool CanSetType(TileTypeEnum type)
		{
			if ( HasRoad() )
			{
				return false;
			}
			if ( type == TileType )
			{
				return false;
			}

			switch ( type )
			{
				case TileTypeEnum.Business:
					return IsNextToRoad();
					
				case TileTypeEnum.House:
					return IsNextToRoad();
			}

			return true;
		}


		public List<GridSpace> GetConnectedTiles(TileTypeEnum type)
		{
			var items = new List<GenericTile>();
			var gridItems = Map.GetGridAsList();
			return gridItems.FindAll( ( item ) =>
			{
				if ( item is GenericTile )
				{
					var roadTile = (GenericTile)item;
					if ( roadTile.TileType == type )
					{
						return Map.IsPath( GridPosition, item.GridPosition );
					}
					return false;
				}
				else
				{
					return false;
				}
			} );
		}

		public GenericTile GetRandomConnectedTile( TileTypeEnum type)
		{
			var connectedTiles = GetConnectedTiles( type );
			if ( connectedTiles.Count == 0)
			{
				return null;
			}

			var index = Rand.Int( 0, connectedTiles.Count - 1 );
			return (GenericTile) connectedTiles[index];
		}
	}
}
