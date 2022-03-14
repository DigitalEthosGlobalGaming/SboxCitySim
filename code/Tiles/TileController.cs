
using static CitySim.GenericTile;

namespace CitySim
{
	public partial class TileController 
	{

		public enum Direction
		{
			Up = 0,
			Right = 1,
			Down = 2,
			Left = 3,
		}

		public static float GetDegreesFromDirection( Direction dir )
		{

			switch ( dir )
			{
				case Direction.Up:
					return 270f;
				case Direction.Right:
					return 0f;
				case Direction.Down:
					return 90f;
				case Direction.Left:
					return 180f;
				default:
					return 0f;
			}
		}
		public static Rotation GetRotationFromDirection(Direction dir)
		{
			var degrees = GetDegreesFromDirection( dir );
			return  Rotation.FromAxis( Vector3.Up, degrees );
		}

		public virtual TileTypeEnum GetTileType()
		{
			return TileTypeEnum.Base;
		}

		public virtual bool CanMoveOn(MovementEntity ent)
		{
			return ent != null;
		}

		public virtual void OnNeighbourTileControllerChange( GenericTile tile, Direction dir, TileController prev, TileController next)
		{

		}


		public GenericTile Parent { get; set; }

		public virtual bool CanAddToTile( GenericTile tile )
		{
			if ( tile.Parent == null )
			{
				return false;
			}

			return true;
		}

		public virtual void AddToTile(GenericTile tile)
		{
			UpdateModel();
		}

		public virtual void RemoveFromTile( GenericTile tile )
		{
			this.Parent = null ;
		}

		public virtual void UpdateModel()
		{
			Parent.SetModel( "models/roads/street_empty.vmdl" );
		}
	}


}
