using GridSystem;
using Sandbox;
using System.Collections;
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

		public static string GetModelForTileType( TileTypeEnum type )
		{
			TileTypeModels = new string[]{ "models/roads/street_4way.vmdl", "models/buildings/forest.vmdl", "models/buildings/shop.vmdl", "models/buildings/house_01.vmdl", "models/roads/street_4way.vmdl" };
			return TileTypeModels[(int)type] ?? "models/roads/street_4way.vmdl";
		}


		[Net]
		public TileTypeEnum TileType { get; set; }
		[Net]
		public int bodyIndex { get; set; } = 0;

		[Net]
		public int materialIndex { get; set; } = 0;

		public void UpdateModel(int? bodyIndex = null, int? materialIndex = null)
		{
			if (bodyIndex != null)
			{
				this.bodyIndex = bodyIndex.Value;
			}

			if ( materialIndex  != null)
			{
				this.materialIndex = materialIndex.Value;
			}

			GenericTile.UpdateModel( this, TileType, bodyIndex ?? this.bodyIndex, materialIndex ?? this.materialIndex );
		}
		public static void UpdateModel( ModelEntity entity, TileTypeEnum type, int bodyIndex, int materialIndex )
		{
			var model = GetModelForTileType( type );
			if (model != "")
			{
				if ( model != entity.Model.Name )
				{
					entity.SetModel( model );
					entity.SetupPhysicsFromModel( PhysicsMotionType.Static, false );
					entity.SetBodyGroup( "base", bodyIndex );
					entity.SetMaterialGroup( materialIndex );
				}
			}
			entity.RenderColor = Color.White;
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

		public int SetTileType( TileTypeEnum type, int bodyIndex, int materialIndex )
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

				IsNeedsSetup = false;

				CheckNeighbours();
				if ( type == TileTypeEnum.Road )
				{
					CheckModel();
				}
				else
				{
					UpdateModel( bodyIndex, materialIndex );
				}

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
