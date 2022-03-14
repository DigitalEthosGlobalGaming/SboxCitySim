
using Sandbox;

namespace CitySim
{
	public partial class BuildingTileController: TileController
	{
		public ModelEntity Building { get; set; }

		public override void AddToTile(GenericTile tile)
		{
			base.AddToTile(tile);

			if ( Building != null)
			{
				Building.Delete();
			}

			Building = new ModelEntity();
			Building.Position = tile.Position;

			UpdateModel();
		}

		public void SetBuildingModel(string model)
		{
			if (Building != null)
			{
				Building.SetModel( model );
				Building.Scale = RoadMap.TileScale;
			}
		}

		public void MakeBuildingFaceRoad()
		{
			var roadNeighbour = this.Parent.GetRoadNeighbour();
			if ( roadNeighbour != null )
			{
				var direction = roadNeighbour?.Key;
				var rotation = GetRotationFromDirection( direction.GetValueOrDefault() );
				Building.Rotation = rotation;
			}
		}

		public override void RemoveFromTile( GenericTile tile )
		{
			base.RemoveFromTile( tile );
			if ( Building != null )
			{
				Building.Delete();
				Building = null;
			}
			this.UpdateModel();
		}
	}


}
