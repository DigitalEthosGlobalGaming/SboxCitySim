using static CitySim.GenericTile;

namespace CitySim
{
	public partial class BusinessTileController : BuildingTileController
	{

		public override TileTypeEnum GetTileType()
		{
			return TileTypeEnum.Business;
		}

		public override void AddToTile(GenericTile tile)
		{
			base.AddToTile( tile );
			MakeBuildingFaceRoad();
			SetBuildingModel( "models/buildings/shop.vmdl" );
		}


		public override bool CanAddToTile(GenericTile tile)
		{
			var roadNeighbour = tile.GetRoadNeighbour();

			return (roadNeighbour != null);
		}

	}
}
