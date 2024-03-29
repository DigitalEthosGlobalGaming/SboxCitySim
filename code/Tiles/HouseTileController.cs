﻿
using Sandbox;
using System.Collections.Generic;
using static CitySim.GenericTile;

namespace CitySim
{
	public partial class HouseTileController : BuildingTileController
	{
		public int BodyGroupIndex { get; set; } = -1;
		public int MaterialIndex { get; set; } = -1;

		public HouseTileController() : base( "models/buildings/house_01.vmdl" )
		{
			CreateNeed( "food", 2f, Game.Random.Float( 18, 30 ) );

			BodyGroupIndex = Game.Random.Int( 0, 4 );
			MaterialIndex = Game.Random.Int( 0, 6 );
		}
		public override TileTypeEnum GetTileType()
		{
			return TileTypeEnum.House;
		}

		public override void AddToTile( GenericTile tile )
		{
			base.AddToTile( tile );

			MakeBuildingFaceRoad();

			Building?.SetBodyGroup( "base", BodyGroupIndex );
			Building?.SetMaterialGroup( MaterialIndex );
		}

		public override bool CanAddToTile( GenericTile tile )
		{
			if ( !base.CanAddToTile( tile ) )
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
			base.Deserialize( data );

			BodyGroupIndex = data.GetValueOrDefault( "BodyGroupIndex", "0" ).ToInt();
			MaterialIndex = data.GetValueOrDefault( "MaterialIndex", "0" ).ToInt();

		}


	}
}
