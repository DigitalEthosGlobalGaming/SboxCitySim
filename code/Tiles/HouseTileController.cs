
using static CitySim.GenericTile;

namespace CitySim
{
	public partial class HouseTileController : TileController
	{


		public override TileTypeEnum GetTileType()
		{
			return TileTypeEnum.House;
		}

		public override void AddToTile(GenericTile tile)
		{
			base.AddToTile(tile);
			var roadNeighbour = tile.GetRoadNeighbour();
			if ( roadNeighbour != null )
			{
				var direction = roadNeighbour?.Key;
				var rotation = GetRotationFromDirection( direction.GetValueOrDefault() );
				Parent.TargetRotation = rotation;
			}
		}

		public override bool CanAddToTile(GenericTile tile)
		{
			var roadNeighbour = tile.GetRoadNeighbour();

			return (roadNeighbour != null);
		}


		public override void UpdateModel()
		{
			Parent.SetModel( "models/buildings/house_01.vmdl" );
		}
	}
}
