using static CitySim.GenericTile;

namespace CitySim
{
	public partial class BusinessTileController : TileController
	{

		public override TileTypeEnum GetTileType()
		{
			return TileTypeEnum.Business;
		}

		public override void AddToTile(GenericTile tile)
		{
			base.AddToTile(tile);
			var roadNeighbour = tile.GetRoadNeighbour();
			if ( roadNeighbour != null )
			{
				var direction = roadNeighbour?.Key;
				var rotation = GetRotationFromDirection( direction.GetValueOrDefault() );
				this.Parent.TargetRotation = rotation;
			}


			// Todo make it rotation the correct place.
			// this.Parent.TargetRotation = this.Parent.Position
		}

		public override bool CanAddToTile(GenericTile tile)
		{
			var roadNeighbour = tile.GetRoadNeighbour();

			return (roadNeighbour != null);
		}


		public override void UpdateModel()
		{
			Parent.SetModel( "models/buildings/shop.vmdl" );
		}
	}
}
