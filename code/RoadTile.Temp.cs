using GridSystem;
using Sandbox;
using System.Collections;
using System.Collections.Generic;

namespace CitySim
{
	partial class RoadTile : GridSpace, ITickable
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
			TileTypeModels = new string[]{ "", "models/buildings/forest.vmdl", "models/buildings/shop.vmdl", "models/buildings/house_01.vmdl", "models/roads/street_4way.vmdl" };
			return TileTypeModels[(int)type] ?? "models/roads/street_4way.vmdl";
		}


		[Net]
		public TileTypeEnum TileType { get; set; }

		public void UpdateModel()
		{
			var model = GetModelForTileType(TileType);
			if (model != "")
			{
				if ( model != CurrentModel )
				{
					SetModel( model );
					SetupPhysicsFromModel( PhysicsMotionType.Static, false );
					if ( TileType == TileTypeEnum.Business || TileType == TileTypeEnum.House )
					{
						SetBodyGroup( "base", Rand.Int( 0, 4 ) );
						var numbers = new int[] { 0,90,180,270 };
						Rotation = Rotation.FromAxis(Vector3.Up, Rand.FromArray( numbers ));
					}
					CurrentModel = model;
				}
			}
			RenderColor = Color.White;
		}



		public void CheckNeighbours()
		{

			RoadTile[] neighbours = GetNeighbours<RoadTile>();
			neighbours[0]?.CheckModel();
			neighbours[1]?.CheckModel();
			neighbours[2]?.CheckModel();
			neighbours[3]?.CheckModel();
		}

		public bool SetTileType( TileTypeEnum type)
		{
			if (IsServer)
			{
				if (!CanSetType(type))
				{
					return false;
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
					UpdateModel();
				}

				MyGame.GetMap().UpdateScore();
				return true;
			}
			return false;
		}

		public bool IsNextToRoad()
		{
			var neighbours = GetNeighbours<RoadTile>();
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
			var items = new List<RoadTile>();
			var gridItems = Map.GetGridAsList();
			return gridItems.FindAll( ( item ) =>
			{
				if ( item is RoadTile )
				{
					var roadTile = (RoadTile)item;
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

		public RoadTile GetRandomConnectedTile( TileTypeEnum type)
		{
			var connectedTiles = GetConnectedTiles( type );
			if ( connectedTiles.Count == 0)
			{
				return null;
			}

			var index = Rand.Int( 0, connectedTiles.Count - 1 );
			return (RoadTile) connectedTiles[index];
		}
	}
}
