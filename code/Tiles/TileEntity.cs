namespace Sandbox.Tiles
{

	/// <summary>
	/// Allows access to certain physics options from Entity Prefabs.
	/// </summary>
	[Prefab]
	[Title( "Placeable" )]
	public class TileEntity : AnimatedEntity
	{
	}

	/// <summary>
	/// Allows access to certain physics options from Entity Prefabs.
	/// </summary>
	[Prefab]
	[Title( "Need" )]
	public class TileNeed : EntityComponent
	{
		[Prefab]
		public string Type { get; set; }

		[Prefab]
		public RangedFloat Amount { get; set; }

		[Prefab]
		[Description( "The max amount of this resource that it can create." )]
		public float MaxSupply { get; set; }
	}

	/// <summary>
	/// Allows access to certain physics options from Entity Prefabs.
	/// </summary>
	[Prefab]
	[Title( "Tile Parking Spot" )]
	public class TileParkingSpot : EntityComponent
	{

		[Prefab]
		[Property]
		[Editor( "test" )]
		public Vector3 ParkingPosition { get; set; }
	}



}
