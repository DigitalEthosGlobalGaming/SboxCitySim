using Sandbox;
using System.Collections.Generic;
using static CitySim.GenericTile;

namespace CitySim
{
	public partial class BusinessTileController : BuildingTileController
	{

		public int BodyGroupIndex { get; set; } = -1;
		public int MaterialIndex { get; set; } = -1;
		public BusinessTileController() : base( "models/buildings/shop.vmdl" )
		{

			Needs = new TileNeeds( 5, 20 )
			{
				NeedType = TileNeeds.TileNeedsType.Food,
				MaxSupply = Rand.Int( 18, 30 ),
				Demand = 2
			};

			BodyGroupIndex = Rand.Int( 0, 3 );
			MaterialIndex = Rand.Int( 0, 6 );
		}

		public override TileTypeEnum GetTileType()
		{
			return TileTypeEnum.Business;
		}

		public override void AddToTile(GenericTile tile)
		{
			base.AddToTile( tile );

			MakeBuildingFaceRoad();

			Building?.SetBodyGroup( "base", BodyGroupIndex );
			Building?.SetMaterialGroup( MaterialIndex );
		}


		public override bool CanAddToTile(GenericTile tile)
		{
			if (!base.CanAddToTile(tile))
			{
				return false;
			}

			var roadNeighbour = tile.GetRoadNeighbour();

			return (roadNeighbour != null);
		}

		public override Dictionary<string, string> Serialize()
		{
			var data = base.Serialize();
			data["BodyGroupIndex"] = BodyGroupIndex.ToString();
			data["MaterialIndex"] = MaterialIndex.ToString();

			return data;
		}

		public override void Deserialize( Dictionary<string, string> data )
		{
			base.Deserialize( data);

			BodyGroupIndex = data.GetValueOrDefault( "BodyGroupIndex", "0").ToInt();
			MaterialIndex = data.GetValueOrDefault( "MaterialIndex", "0" ).ToInt();

		}

	}
}
