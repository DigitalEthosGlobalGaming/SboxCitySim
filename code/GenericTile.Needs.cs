using CitySim.Utils;
using Degg.GridSystem;
using Sandbox;
using System.Collections.Generic;

namespace CitySim
{
	public partial class GenericTile : GridSpace, ITickable
	{
		private bool isDirty = false;
		public bool IsDirty
		{
			get 
			{
				// Quantum Physics! Once observed this will no longer be dirty.
				// Note: We may want to have an additional boolean that would track that it was observed; and then set this to false at the end of a frame or next frame.
				//			That or do not do this at all...
				bool oldVal = isDirty;
				isDirty = false;
				return oldVal; 
			}
			set { isDirty = value; }
		}

		public bool HasNeeds { get; set; }
		public int FoodSupply { get; set; } = 0;
		public int FoodDemand { get; set; }
		public int FoodNeedMax { get; set; } = 20;
		public int FoodNeedMin { get; set; } = 5;
		public List<MovementEntity> DeliveryEntities { get; set; } = new List<MovementEntity>();
		/*
				float NeedsNextTick = 0;
				float NeedsInterval = 5f;
		*/
		//public bool IsNeedsSetup { get; set; } = false;
		public void SetupNeeds()
		{
			if ( TileType == TileTypeEnum.House )
			{
				HasNeeds = true;
				FoodDemand = Rand.Int( 1, 3 );
				FoodSupply = Rand.Int( FoodNeedMin, FoodNeedMax );
			}
			else if ( TileType == TileTypeEnum.Business )
			{
				HasNeeds = true;
				FoodDemand = Rand.Int( 1, 3 );
				FoodSupply = Rand.Int( FoodNeedMin, FoodNeedMax );
			}
		}

		/*
		


		public void UpdateFoodNeed()
		{
			FoodSupply = FoodSupply - FoodDemand;
			if ( FoodSupply <= 0 )
			{
				FoodSupply = 0;
				FixFoodNeed();
			}
		}

		public void FixFoodNeed()
		{
			if ( TileType == TileTypeEnum.House )
			{
				if ( DeliveryEntity == null )
				{
					var start = GetRoadNeighbour();
					if ( start == null )
					{
						return;
					}
					var businessTile = start.GetRandomConnectedTile( TileTypeEnum.Business );
					if ( businessTile != null )
					{
						// Building isn't connected to a road.
						if ( start == null )
						{
							return;
						}
						var map = ((MyGame)Game.Current).Map;
						var path = map.CreatePath( start.GridPosition, businessTile.GridPosition );
						var ent = new MovementEntity();
						ent.Init( path, true );

						ent.OnFinishEvents.Enqueue( () =>
						{
							DeliveryEntity = null;
							FoodSupply = Rand.Int( FoodNeedMin, FoodNeedMax );
							return true;
						} );
						DeliveryEntity = ent;
					}
				}
			} 
			else if (TileType == TileTypeEnum.Business)
			{
				var tiles = Map.GetTilesAtEdgeOfMap<GenericTile>();
				tiles = tiles.FindAll( ( tile ) => { return ((GenericTile)tile).TileType == TileTypeEnum.Road && ((GenericTile)tile).RoadType == RoadTypeEnum.DeadEnd; } );

				var myPosition = this.GridPosition;
				var road = this.GetRoadNeighbour();
				tiles = tiles.FindAll( ( tile ) =>
				{
					var canMove = Map.IsPath( tile.GridPosition, myPosition  );
					return canMove;
				} );

				var start = (GenericTile) Rand.FromList<GenericTile>( tiles );


				if ( start == null )
				{
					return;
				}

				var end = GridPosition;

				// Building isn't connected to a road.
				if ( start == null )
				{
					return;
				}
				var map = ((MyGame)Game.Current).Map;
				var path = map.CreatePath( start.GridPosition, GridPosition );
				var ent = new MovementEntity();
				ent.SetBodyGroup( "base", 6 );

				ent.Init( path, true );

				ent.OnFinishEvents.Enqueue( () =>
				{
					DeliveryEntity = null;
					FoodSupply = Rand.Int( FoodNeedMin, FoodNeedMax );
					return true;
				} );

				DeliveryEntity = ent;
			}
		}

		public void UpdateNeeds()
		{
			if ( IsNeedsSetup )
			{
				if ( HasNeeds )
				{
					if ( NeedsNextTick < Time.Now )
					{
						NeedsNextTick = Time.Now + NeedsInterval;
						UpdateFoodNeed();
					}
				}
			}
			else
			{
				SetupNeeds();
			}
		}
		*/
	}
}
