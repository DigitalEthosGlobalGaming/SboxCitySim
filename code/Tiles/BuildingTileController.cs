﻿
using CitySim.Utils;
using Sandbox;

namespace CitySim
{
	public partial class BuildingTileController: TileController, ITickable
	{
		public ModelEntity Building { get; set; }


		public Vector3 TargetPosition { get; set; }

		public bool MakesTileHaveNeeds { get; set; }

		public const float SpawnHeight = 25f;

		public bool HideParent { get; set; }
		public TickableCollection ParentCollection { get; set; }

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
				Building.Position = tile.Position + ( Vector3.Up * SpawnHeight );
				TargetPosition = tile.Position;
				tile.EnableDrawing = !HideParent;
				TickableCollection.Global.Add( this );
			} else
			{				
				Building.Position = tile.Position + (Vector3.Up * SpawnHeight);
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


		void ITickable.OnClientTick( float delta, float currentTick )
		{
			throw new System.NotImplementedException();
		}
		
		public void SetBuildingPosition(Vector3 pos)
		{
			if ( ParentCollection == null)
			{
				TickableCollection.Global.Add( this );
			}

			Building.Position = pos;
		}

		void ITickable.OnServerTick( float delta, float currentTick )
		{			
			if ( Building != null )
			{
				var distance = 25f * delta;

				Building.Position = Building.Position.LerpTo( TargetPosition, distance, true );
				if ( Building.Position.z == TargetPosition.z )
				{
					TickableCollection.Global.Remove( this );
				}
			}
		}

		void ITickable.OnSharedTick( float delta, float currentTick )
		{
			
		}
	}


}
