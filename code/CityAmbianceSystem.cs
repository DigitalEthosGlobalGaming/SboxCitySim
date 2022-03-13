using Degg.GridSystem;
using Sandbox;
using System.Collections.Generic;

namespace CitySim
{
	public enum CityAmbianceVehicleSpawnType
	{
		Edge,
		Tile
	}
	public struct CityAmbianceVehicleBehaviour
	{
		// All Vehicles are living in the normalcar1 model.
		// Thus we can select the body group index via this variable.
		public uint bodyIndex;

		// Is this vehicle allow to go back to it's original spawning tile
		public bool shouldReturn;

		public int randFoodCapacityMin;
		public int randFoodCapacityMax;

		// Tell the City Ambiance System, where you want to spawn this vehicle type.
		public CityAmbianceVehicleSpawnType spawnFromType;
		
		// This will determine which tile that a vehicle will spawn from.
		public GenericTile.TileTypeEnum spawnTileType;
		// This integer is applied to sub catagories like Spawn Tile Road, and Road Type of any type.
		public int userSpawnTileIndex;

		// This will determine which tile that a vehicle will go to after spawning.
		// Note: If the spawn and delivery tile types are the same, it will goto the next available tile of type.
		public GenericTile.TileTypeEnum deliveryTileType;
	}

	public class CityAmbianceSystem
	{
		public static uint MAX_VEHICLE_LIMIT = 100;
		public static int MAX_VEHICLE_SPAWN_RATE = 3;
 		private static float TICKINTERVAL = 5f;

		public int Deliverers 
		{
			get
			{
				int count = 0;
				foreach ( var ent in MovementEntity.All )
				{
					if (ent is MovementEntity)
					{
						count++;
					}
				}
				return count;
			} 
		}

		private float nextTick = 0;

		private RoadMap roadMap;
		
		public CityAmbianceSystem(RoadMap _roadMap)
		{
			roadMap = _roadMap;
		}

		private List<GenericTile> GetTilesInNeed()
		{
			// Todo :: Cache and dynamically add and remove the generic tiles that have needs.
			List<GenericTile> tiles = roadMap.GetGenericTiles();
			tiles.FindAll( ( tile ) =>
			{
				return tile.HasNeeds && 
				tile.FoodSupply <= 0 && 
				tile.DeliveryEntities.Count == 0;
			} );

			return tiles;
		}

		private List<GenericTile> CalcalateTilesInNeed()
		{
			List<GenericTile> listInNeed = new List<GenericTile>();
			// Scan all the Grids and update their Food Supplies.
			foreach (var grid in roadMap.Grid)
			{
				if ( grid is GenericTile tile )
				{
					if (tile.HasNeeds)
					{
						tile.FoodSupply -= tile.FoodDemand;

						if (tile.FoodSupply <= 0)
						{
							tile.FoodSupply = 0;
							listInNeed.Add(tile);
						}
					}
				}
			}

			return listInNeed;
		}

		public void Update()
		{
			if (nextTick > Time.Now)
			{
				// Not ready to execute the this tick.
				return;
			}

			// Ready to execute this tick.
			nextTick = Time.Now + TICKINTERVAL;

			List<GenericTile> tilesInNeed = CalcalateTilesInNeed();

			// Spawn the maximum amount of tiles in need, within the range of the MAX_VEHICLE_SPAWN_RATE.
			for ( var i = 0; i < System.Math.Min( tilesInNeed.Count, MAX_VEHICLE_SPAWN_RATE ); i++ )
			{
				int randNum = Rand.Int( 100 );

				if (randNum < 25)
				{
					SpawnVehicle( new CityAmbianceVehicleBehaviour()
					{
						bodyIndex = 6,
					
						deliveryTileType = GenericTile.TileTypeEnum.Business,
					
						randFoodCapacityMax = 6,
						randFoodCapacityMin = 3,

						shouldReturn = true,

						spawnFromType = CityAmbianceVehicleSpawnType.Edge,
						spawnTileType = GenericTile.TileTypeEnum.Road,
						userSpawnTileIndex = (int)GenericTile.RoadTypeEnum.DeadEnd
					} );
				}
				else
				{
					SpawnVehicle( new CityAmbianceVehicleBehaviour()
					{
						bodyIndex = (uint)Rand.Int(0, 4),

						deliveryTileType = GenericTile.TileTypeEnum.Business,

						randFoodCapacityMax = 6,
						randFoodCapacityMin = 3,

						shouldReturn = true,

						spawnFromType = CityAmbianceVehicleSpawnType.Tile,
						spawnTileType = GenericTile.TileTypeEnum.House
					} );
				}
			}

			// Spawn Random Ambulances
			var hospitalTiles = roadMap.GetGenericTiles().FindAll( ( tile ) => {
				tile.BodyGroups.TryGetValue( "base", out int tileBaseIndex );

				return tile.TileType == GenericTile.TileTypeEnum.Business &&
						tileBaseIndex == 2; 
			} );
			int ambulanceCount = 0;
			foreach ( var hospitalTile in hospitalTiles )
			{
				if (ambulanceCount < MAX_VEHICLE_SPAWN_RATE/2)
				{
					int randNum = Rand.Int( 100 );
					if ( randNum <= 5 )
					{
						SpawnVehicle<AmbulanceEntityMovement>( new CityAmbianceVehicleBehaviour()
						{
							bodyIndex = 5,

							deliveryTileType = GenericTile.TileTypeEnum.House,

							randFoodCapacityMin = 0,
							randFoodCapacityMax = 0,

							shouldReturn = true,

							spawnFromType = CityAmbianceVehicleSpawnType.Tile,

							spawnTileType = GenericTile.TileTypeEnum.Business,
							userSpawnTileIndex = 2
						} );
					}
					ambulanceCount++;
				}
				else
				{
					break;
				}
			}
		}

		public void SpawnVehicle( CityAmbianceVehicleBehaviour behaviour )
		{
			SpawnVehicle<MovementEntity>( behaviour );
		}

		public void SpawnVehicle<T>( CityAmbianceVehicleBehaviour behaviour ) where T : MovementEntity, new()
		{
			if ( Deliverers >= MAX_VEHICLE_LIMIT )
			{
				return;
			}

			List<GenericTile> possibleTiles = new List<GenericTile>();
			
			// Create a list of possible tiles that the later logic can determine which one will be picked for spawning and delivering the package.
			switch(behaviour.spawnFromType)
			{
				case CityAmbianceVehicleSpawnType.Edge:
					{
						List<GenericTile> tiles = roadMap.GetTilesAtEdgeOfMap<GenericTile>();
						tiles = tiles.FindAll( ( tile ) => {
							return tile.TileType == behaviour.spawnTileType &&
							tile.RoadType == (GenericTile.RoadTypeEnum)behaviour.userSpawnTileIndex;
						} );

						possibleTiles.AddRange( tiles );

						break;
					}
				case CityAmbianceVehicleSpawnType.Tile:
					{
						List<GenericTile> tiles = roadMap.GetGenericTiles();
						tiles = tiles.FindAll( ( tile ) => {
							return tile.TileType == behaviour.spawnTileType;
						} );

						possibleTiles.AddRange( tiles );

						break;
					}
				default:
					{
						Log.Error( "Unsupported Spawn Location Type when creating a Vehicle." );
						break;
					}
			}

			// Verify that we have possible places to spawn.
			if (possibleTiles.Count == 0)
			{
				return;
			}

			// After we know the spawn points we can start looking for delivery points, so we can calculate a point to goto.
			List<GenericTile> deliveryTiles = roadMap.GetGenericTiles();
			deliveryTiles = deliveryTiles.FindAll( ( tile ) => {
				return tile.HasNeeds && 
				tile.FoodSupply <= 0 && 
				tile.TileType == behaviour.deliveryTileType && 
				tile.IsNextToRoad();
			} );

			// Verify that we have places to deliver.
			if (deliveryTiles.Count == 0)
			{
				return;
			}

			GenericTile deliveryTile = Rand.FromList( deliveryTiles );

			// Finally we know a list of places we are allowed to spawn, we need to determine if we want to spawn there.
			possibleTiles = possibleTiles.FindAll( ( tile ) =>
			{
				return tile.IsNextToRoad() && roadMap.IsPath( tile.GetRoadNeighbour().GridPosition, deliveryTile.GridPosition );
			} );

			// Verify that we still have places to spawn.
			if (possibleTiles.Count == 0)
			{
				return;
			}

			GenericTile spawnTile = Rand.FromList( possibleTiles );

			var path = roadMap.CreatePath( spawnTile.GetRoadNeighbour().GridPosition, deliveryTile.GridPosition );

			MovementEntity ent = new T();
			ent.SetBodyGroup( "base", (int)behaviour.bodyIndex );

			ent.OnFinishEvents.Enqueue( () =>
			{
				deliveryTile.FoodSupply += Rand.Int( behaviour.randFoodCapacityMin, behaviour.randFoodCapacityMax );
				deliveryTile.FoodSupply = (int)MathX.Clamp( deliveryTile.FoodSupply, 0, deliveryTile.FoodNeedMax );

				deliveryTile.DeliveryEntities.Remove(ent);

				return true;
			} );

			deliveryTile.DeliveryEntities.Add(ent);
			ent.Init( path, behaviour.shouldReturn );
		}
	}
}
