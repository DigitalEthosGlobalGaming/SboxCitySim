using GridSystem;
using Sandbox;
using System.Collections;
using System.Collections.Generic;

namespace CitySim
{
	public partial class RoadTile : GridSpace, ITickable
	{

		public bool HasNeeds { get; set; }
		int FoodNeeds { get; set; } = 0;
		int FoodNeedAmount { get; set; }
		int FoodNeedMax { get; set; } = 30;
		int FoodNeedMin { get; set; } = 10;
		public MovementEntity FoodNeedEntity { get; set; }
		float NeedsNextTick = 0;
		float NeedsInterval = 5f;


		public bool IsNeedsSetup { get; set; } = false;
		public void SetupNeeds()
		{
			if ( TileType == TileTypeEnum.House )
			{
				HasNeeds = true;
				FoodNeedAmount = Rand.Int( 1, 3 );
				FoodNeeds =  Rand.Int( FoodNeedMin, FoodNeedMax );
			}
			IsNeedsSetup = true;
		}


		public void UpdateFoodNeed()
		{
			FoodNeeds = FoodNeeds - FoodNeedAmount;
			if ( FoodNeeds <= 0 )
			{
				FoodNeeds = 0;
				FixFoodNeed();
			}
		}

		public void FixFoodNeed()
		{
			if ( FoodNeedEntity == null )
			{
				var start = GetRoadNeighbour();
				if (start == null)
				{
					return;
				}
				var businessTile = start.GetRandomConnectedTile( TileTypeEnum.Business );
				if ( businessTile != null )
				{	
					// Building isn't connected to a road.
					if (start == null)
					{
						return;
					}
					var map = ((MyGame)Game.Current).Map;
					var path = map.CreatePath( start.GridPosition, businessTile.GridPosition );
					var ent = new MovementEntity();
					ent.Init( path, true );

					ent.OnFinishEvents.Enqueue(() => {
						FoodNeedEntity = null;
						FoodNeeds = Rand.Int( FoodNeedMin, FoodNeedMax );
						return true; 
					});
					FoodNeedEntity = ent;
				}
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
	}
}
