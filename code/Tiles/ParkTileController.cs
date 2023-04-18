using Sandbox;
using static CitySim.GenericTile;

namespace CitySim
{
	public partial class ParkTileController : BuildingTileController
	{

		public ParkTileController() : base( "models/buildings/forest.vmdl" )
		{
			HideParent = true;
		}

		public override TileTypeEnum GetTileType()
		{
			return TileTypeEnum.Park;
		}

		public override void AddToTile( GenericTile tile )
		{
			base.AddToTile( tile );

			Building.SetBodyGroup( "rock1", Game.Random.Int( 0, 2 ) );
			Building.SetBodyGroup( "rock2", Game.Random.Int( 0, 2 ) );
			Building.SetBodyGroup( "rock3", Game.Random.Int( 0, 2 ) );
			Building.SetBodyGroup( "bush1", Game.Random.Int( 0, 2 ) );
			Building.SetBodyGroup( "bush2", Game.Random.Int( 0, 2 ) );
			Building.SetBodyGroup( "bush3", Game.Random.Int( 0, 2 ) );
			if ( Game.IsServer )
			{
				Parent.EnableDrawing = false;
			}
		}
		public override void RemoveFromTile( GenericTile tile )
		{
			base.RemoveFromTile( tile );
			if ( Game.IsServer )
			{
				Parent.EnableDrawing = true;
			}
		}
	}
}
