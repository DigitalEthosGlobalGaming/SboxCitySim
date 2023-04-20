
using CitySim.Utils;
using NeedsLibrary;
using Sandbox;

namespace CitySim
{
	public partial class BuildingTileController : TileController, ITickable
	{
		public ModelEntity Building { get; set; }
		public string BuildingModel { get; private set; }
		public Vector3 TargetPosition { get; set; }

		public bool MakesTileHaveNeeds { get; set; }

		public const float SpawnHeight = 25f;

		public bool HideParent { get; set; }
		public TickableCollection ParentCollection { get; set; }

		public Consumer MyConsumer { get; set; }

		public BuildingTileController( string _buildModel )
		{
			BuildingModel = _buildModel;
			if ( Game.IsServer )
			{
				var store = NeedsLibrary.Needs.GetOrCreateStore( "Game" );
				MyConsumer = store.CreateConsumer( this );
			}
		}

		public override void AddToTile( GenericTile tile )
		{
			base.AddToTile( tile );

			if ( Building != null )
			{
				Building.Delete();
			}

			Building = new ModelEntity();
			Building.Transmit = TransmitType.Always;

			SetBuildingModel( BuildingModel );

			if ( Game.IsServer )
			{
				Building.Position = tile.Position + (Vector3.Up * SpawnHeight);
				TargetPosition = tile.Position;
				tile.EnableDrawing = !HideParent;
				TickableCollection.Global.Add( this );
			}
			else
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
			if ( Building != null )
			{
				Building.EnableDrawing = visible;
			}
		}


		public override void Destroy()
		{
			Building?.Delete();
			if ( Game.IsServer )
			{
				MyConsumer.Delete();
			}
		}

		public void CheckConsumer()
		{
			if ( Game.IsServer )
			{
				if ( MyConsumer == null )
				{
					MyConsumer = NeedsLibrary.Needs.GetOrCreateStore( "citysim" ).CreateConsumer( this );
				}
			}
		}

		public void CreateNeed( string type, float demand, float maxSupply )
		{
			if ( Game.IsServer )
			{
				CheckConsumer();

				MyConsumer.CreateDemand( type, demand, maxSupply );
			}
		}

		public void CreateSupply( string type, float amount, float maxSupply )
		{
			if ( Game.IsServer )
			{
				CheckConsumer();
				// MyConsumer.CreateProduce( type, amount, maxSupply );

			}
		}

		public void SetBuildingModel( string model )
		{
			if ( Building != null )
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

			if ( Game.IsServer )
			{
				tile.EnableDrawing = true;
			}

			this.UpdateModel();
		}


		void ITickable.OnClientTick( float delta, float currentTick )
		{
			throw new System.NotImplementedException();
		}

		public void SetBuildingPosition( Vector3 pos )
		{
			if ( ParentCollection == null )
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
