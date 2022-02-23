using GridSystem;

namespace CitySim
{
	public partial class RoadMap : GridMap
	{
		public void Init()
		{
			Init<RoadTile>( new Vector3( 0, 0, 1000 ), new Vector2( 200, 200 ), 5, 5 );
		}
	}

}
