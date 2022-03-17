using CitySim.UI;
using CitySim.Utils;
using Degg.GridSystem;
using Sandbox;
using static CitySim.GenericTile;

namespace CitySim
{
	public partial class ParkTileController : BuildingTileController
	{

		public ParkTileController(): base()
		{
			HideParent = true;
		}

		public override TileTypeEnum GetTileType()
		{
			return TileTypeEnum.Park;
		}

		public override void AddToTile(GenericTile tile)
		{
			base.AddToTile(tile);
			
			SetBuildingModel( "models/buildings/forest.vmdl" );
			Building.SetBodyGroup( "rock1", Rand.Int( 0, 2 ) );
			Building.SetBodyGroup( "rock2", Rand.Int( 0, 2 ) );
			Building.SetBodyGroup( "rock3", Rand.Int( 0, 2 ) );
			Building.SetBodyGroup( "bush1", Rand.Int( 0, 2 ) );
			Building.SetBodyGroup( "bush2", Rand.Int( 0, 2 ) );
			Building.SetBodyGroup( "bush3", Rand.Int( 0, 2 ) );
			if ( Parent.IsServer )
			{
				Parent.EnableDrawing = false;
			}
		}
		public override void RemoveFromTile( GenericTile tile )
		{
			base.RemoveFromTile( tile );
			if ( Parent.IsServer )
			{
				Parent.EnableDrawing = true;
			}
		}
	}
}
