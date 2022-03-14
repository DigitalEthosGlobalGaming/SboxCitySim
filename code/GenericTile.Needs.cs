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
			var tileType = GetTileType();
			if ( tileType == TileTypeEnum.House )
			{
				HasNeeds = true;
				FoodDemand = Rand.Int( 1, 3 );
				FoodSupply = Rand.Int( FoodNeedMin, FoodNeedMax );
			}
			else if ( tileType == TileTypeEnum.Business )
			{
				HasNeeds = true;
				FoodDemand = Rand.Int( 1, 3 );
				FoodSupply = Rand.Int( FoodNeedMin, FoodNeedMax );
			}
		}
	}
}
