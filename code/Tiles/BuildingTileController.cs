﻿
using Sandbox;

namespace CitySim
{
	public partial class BuildingTileController: TileController
	{
		public ModelEntity Building { get; set; }

		public bool MakesTileHaveNeeds { get; set; }

		public bool HideParent { get; set; }

		public override void AddToTile(GenericTile tile)
		{
			base.AddToTile(tile);

			if ( Building != null)
			{
				Building.Delete();
			}

			Building = new ModelEntity();
			if ( tile.IsServer )
			{
				Building.Position = tile.Position;
				tile.EnableDrawing = !HideParent;
			} else
			{
				Building.Position = tile.Position + (Vector3.Up * 10f);
				Building.RenderColor = Building.RenderColor.WithAlpha( 0.75f );
			}

			UpdateModel();
		}


		public override bool GetVisible()
		{
			return Building?.EnableDrawing ?? false;
		}

		public override void SetVisible( bool visible )
		{
			if (Building != null)
			{
				Building.EnableDrawing = visible;
			}
		}


		public override void Destroy()
		{
			Building?.Delete();
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

			if ( Parent.IsServer )
			{
				tile.EnableDrawing = true;
			}

			this.UpdateModel();
		}
	}


}