
using Sandbox;
using static CitySim.GenericTile;

namespace CitySim
{
	public partial class TileNeeds 
	{
		public bool IsDelivering = false;
		public enum TileNeedsType
		{
			Food = 0
		}

		public TileNeedsType NeedType { get; set; }

		public int Supply { get; set; }
		public int MaxSupply { get; set; }

		public int Demand { get; set; }

		public bool HasNeeds { get; set; }
		public TileNeeds( int startingSupply = 0 )
		{
			Supply = startingSupply;
		}

		public TileNeeds(int startingMin, int startingMax): this( Rand.Int( startingMin, startingMax ) )
		{

		}
	}


}
