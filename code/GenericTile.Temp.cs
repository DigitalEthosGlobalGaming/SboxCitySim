using CitySim.Utils;
using Degg.GridSystem;
using Sandbox;
using System;
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

		public TileTypeEnum GetTileType() {
			return ControllerId;
		}
		public static bool IsCombination( GenericTile aTile, GenericTile bTile, TileTypeEnum a, GenericTile.TileTypeEnum b, TileTypeEnum? pretenderForA = null )
		{
			var aType = aTile?.GetTileType() ?? TileTypeEnum.Base;
			var bType = bTile?.GetTileType() ?? TileTypeEnum.Base;

			GenericTile.TileTypeEnum aTileType = pretenderForA ?? aType;
			if ( aTileType == a && bType == b )
			{
				return true;
			}
			if ( bType == a && aTileType == b )
			{
				return true;
			}

			return false;
		}

		public int GetTileScore()
		{
			var totalScore = 0;
			foreach ( var neighbourTile in GetNeighbours<GenericTile>() )
			{
				if ( neighbourTile != null )
				{
					totalScore = totalScore + GetTileScore( neighbourTile, this.GetTileType() );
				}
			}

			return totalScore;
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

		public bool IsNextToRoad()
		{
			return GetRoadNeighbour() != null;
		}

		public bool CanSetType(TileController type)
		{
			return type?.CanAddToTile( this ) ?? false;
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
					if ( roadTile.GetTileType() == type )
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
