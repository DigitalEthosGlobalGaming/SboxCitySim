
using static CitySim.GenericTile;

namespace CitySim
{
	public partial class HouseTileController : BuildingTileController
	{
		public override TileTypeEnum GetTileType()
		{
			return TileTypeEnum.House;
		}

		public override void AddToTile(GenericTile tile)
		{
			base.AddToTile(tile);
			MakeBuildingFaceRoad();
			SetBuildingModel( "models/buildings/house_01.vmdl" );
		}

		public override bool CanAddToTile(GenericTile tile)
		{
			var roadNeighbour = tile.GetRoadNeighbour();

			return (roadNeighbour != null);
		}

	}
}
