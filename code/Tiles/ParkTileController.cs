using CitySim.UI;
using CitySim.Utils;
using Degg.GridSystem;
using Sandbox;
using static CitySim.GenericTile;

namespace CitySim
{
	public partial class ParkTileController : TileController
	{

		public override TileTypeEnum GetTileType()
		{
			return TileTypeEnum.Park;
		}

		public override void AddToTile(GenericTile tile)
		{
			base.AddToTile(tile);

			Parent.SetBodyGroup( "rock1", Rand.Int( 0, 2 ) );
			Parent.SetBodyGroup( "rock2", Rand.Int( 0, 2 ) );
			Parent.SetBodyGroup( "rock3", Rand.Int( 0, 2 ) );
			Parent.SetBodyGroup( "bush1", Rand.Int( 0, 2 ) );
			Parent.SetBodyGroup( "bush2", Rand.Int( 0, 2 ) );
			Parent.SetBodyGroup( "bush3", Rand.Int( 0, 2 ) );
		}


		public override void UpdateModel()
		{
			Parent.SetModel( "models/buildings/forest.vmdl" );
		}
	}
}
