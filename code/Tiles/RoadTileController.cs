using CitySim.UI;
using CitySim.Utils;
using Degg.GridSystem;
using Sandbox;
using static CitySim.GenericTile;

namespace CitySim
{
	public partial class RoadTileController : BuildingTileController
	{

		public RoadTypeEnum RoadType { get; set; } = RoadTypeEnum.StreetEmpty;
		public RoadTileController() : base( "models/roads/street_empty.vmdl" )
		{
			HideParent = true;
		}

		public enum RoadTypeEnum
		{
			StreetEmpty = 14,
			Straight = 0,
			Curve = 1,
			ThreeWay = 2,
			FourWay = 4,
			DeadEnd = 11,
			WaterEmpty = 15,
		}

		public override void AddToTile(GenericTile tile)
		{
			base.AddToTile(tile);
		}

		public override TileTypeEnum GetTileType()
		{
			return TileTypeEnum.Road;
		}

		public static string GetModelForRoadType(RoadTypeEnum roadType )
		{
			switch ( roadType )
			{
				case RoadTypeEnum.StreetEmpty:
					return "models/roads/street_empty.vmdl";
				case RoadTypeEnum.Straight:
					return "models/roads/street_straight.vmdl";
				case RoadTypeEnum.Curve:
					return "models/roads/street_curve.vmdl";
				case RoadTypeEnum.ThreeWay:
					return "models/roads/street_3way.vmdl";
				case RoadTypeEnum.FourWay:
					return "models/roads/street_4way.vmdl";
				case RoadTypeEnum.DeadEnd:
					return "models/roads/street_deadend.vmdl";
				case RoadTypeEnum.WaterEmpty:
					return "models/roads/street_water.vmdl";
				default:
					Log.Warning( $"No valid road type for {roadType}" );
					return "models/roads/street_empty.vmdl";
			}
		}


		public override void OnNeighbourTileControllerChange( GenericTile tile, Direction dir, TileController prev, TileController next )
		{
			UpdateModel();
		}

		public override void UpdateModel()
		{
			var influencer = this.Parent;
			float rotation = 0;


			GenericTile[] neighbours = influencer.GetNeighbours<GenericTile>();
			var up = neighbours[0] == null || neighbours[0]?.ControllerId == TileTypeEnum.Road;
			var right = neighbours[1] == null || neighbours[1]?.ControllerId == TileTypeEnum.Road;
			var down = neighbours[2] == null || neighbours[2]?.ControllerId == TileTypeEnum.Road;
			var left = neighbours[3] == null || neighbours[3]?.ControllerId == TileTypeEnum.Road;


			RoadTypeEnum newRoadType = RoadTypeEnum.StreetEmpty;

			// We are a road.
			int totalCount = 0;

			if ( up )
			{
				totalCount++;
			}
			if ( down )
			{
				totalCount++;
			}
			if ( left )
			{
				totalCount++;
			}
			if ( right )
			{
				totalCount++;
			}

			if ( totalCount == 3 ) // THREE WAYS
			{
				newRoadType = RoadTypeEnum.ThreeWay;
				if ( !up )
				{
					rotation = 180;
				}
				else if ( !right )
				{
					rotation = 270;
				}
				else if ( !down )
				{
					rotation = 0;
				}
				else if ( !left )
				{
					rotation = 90;
				}
			}
			else if ( totalCount == 2 ) // STRAIGHT OR CURVES
			{
				if ( left && right )
				{
					newRoadType = RoadTypeEnum.Straight;
					rotation = 90;
				}
				else if ( up && down )
				{
					newRoadType = RoadTypeEnum.Straight;
				}
				else
				{
					newRoadType = RoadTypeEnum.Curve;
					if ( up )
					{
						if ( left )
						{
							rotation = 180;
						}
						else
						{
							rotation = 270;
						}
					}
					if ( down )
					{
						if ( left )
						{
							rotation = 90;
						}
						else
						{
							rotation = 0;
						}
					}
				}
			}
			else if ( totalCount == 1 ) // DEAD ENDS
			{
				if ( up )
				{
					rotation = 0;
				}
				else if ( right )
				{
					rotation = 90;
				}
				else if ( down )
				{
					rotation = 180;
				}
				else if ( left )
				{
					rotation = 270;
				}
				newRoadType = RoadTypeEnum.DeadEnd;
			}
			else
			{
				newRoadType = RoadTypeEnum.FourWay;
			}

			RoadType = newRoadType;


			var TargetRotation = Rotation.FromAxis( Vector3.Up, rotation );

			var model = GetModelForRoadType( newRoadType );

			SetBuildingModel( model );
			if (this.Building != null)
			{
				Building.Rotation = TargetRotation;
			}


		}
	}
}
