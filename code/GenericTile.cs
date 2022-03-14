using CitySim.UI;
using CitySim.Utils;
using Degg.GridSystem;
using Sandbox;
using System.Collections.Generic;
using static CitySim.TileController;

namespace CitySim
{
	public partial class GenericTile : GridSpace, ITickable
	{

		public TileController Controller { get; set; }
		public WorldTileStatUI WorldUI { get; private set; }
		public TickableCollection ParentCollection { get; set; }

		public Rotation TargetRotation { get; set; }
		public Vector3 TargetPosition { get; set; }
		/// <summary>
		/// Called when the entity is first created 
		/// </summary>
		public override void OnAddToMap()
		{
			base.OnAddToMap();
			Position = GetWorldPosition();
			TargetPosition = Position;

			var c = new TileController();
			this.AddController( c );
			UpdateName();
		}

		[ClientRpc]
		public void SpawnUI()
		{
			// Prevent Double Spawn.
			if ( WorldUI != null )
				return;

			if (GetTileType() != TileTypeEnum.Base)
			{
				WorldUI = MyGame.Ui.CreateWorldUi();
			}
		}

		[ClientRpc]
		public void UpdateWorldUI( string _name, int _points = 0 )
		{
			if ( WorldUI == null )
				return;

			WorldUI.Name = _name;
			WorldUI.Points = _points;
		}
		[ClientRpc]
		public void DestroyWorldUI()
		{
			if ( WorldUI == null )
				return;

			WorldUI.Delete();
			WorldUI = null;
		}

		public void UpdateName()
		{
			var suffix = "";
			if (HasRoad())
			{
				suffix = "R";
			}

			Name = $"[{GridPosition.x},{GridPosition.y}] {suffix}";


			UpdateWorldUI( Name );
		}

		public bool HasRoad()
		{
			return this?.Controller is RoadTileController;
		}


		[Event( "citysim.gamestate" )]
		public void OnGameStateChange()
		{
		}



		public override float GetMovementWeight( GridSpace a, NavPoint n )
		{
			if (n.Parent == null)
			{
				return 10f;
			}
			if (a is GenericTile )
			{
				if ( HasRoad() )
				{
					return 10;
				}
			}
				
			return -1;
		}




		public void SetHasRoad(bool hasRoad)
		{
			var c = new RoadTileController();
			AddController( c );
		}

		public KeyValuePair<Direction, GenericTile>? GetRoadNeighbour()
		{
			GenericTile[] neighbours = GetNeighbours<GenericTile>();
			for ( int i = 0; i < neighbours.Length; i++ )
			{
				var tile = neighbours[i];

				if ( tile?.Controller is RoadTileController )
				{
					return KeyValuePair.Create((Direction)i, tile);
				}
			}
			return null;
		}



		public override void Spawn()
		{
			base.Spawn();
			this.Transmit = TransmitType.Always;
			TickableCollection.Global.Add( this );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();
			TickableCollection.Global.Add( this );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if ( ParentCollection != null )
			{
				ParentCollection.Remove( this );
			}
		}


		public void OnClientTick( float delta, float currentTick )
		{
			if (WorldUI != null)
			{
				var position = GetWorldPosition();
				var distance = position.Distance( Pawn.GetClientPawn().EyePosition );
				if (distance == 0)
				{
					distance = 1;
				}
				float scale =  500 / distance;
				WorldUI.SetScale( scale);
				WorldUI.SetPosition( position + (Vector3.Up * 10f) );
			}
		}

		public void OnSharedTick( float delta, float currentTick )
		{

		}

		public void OnServerTick( float delta, float currentTick )
		{
			var transitionAmount = 5f;
			Rotation = Rotation.Slerp( Rotation, TargetRotation, transitionAmount * 2 * delta );
			Position = Position.LerpTo( TargetPosition, transitionAmount * delta );
		}

		public void AddController(TileController t)
		{
			var controller = t;

			var previous = Controller;
			if ( previous != controller && previous != null)
			{
				previous.RemoveFromTile( this );
				previous.Parent = null;
			}

			Controller = controller;
			Controller.Parent = this;
			Controller.AddToTile( this );

			var neighbours = GetNeighbours<GenericTile>();
			for ( int i = 0; i < neighbours.Length; i++ )
			{
				var neighbour = neighbours[i];
				if ( neighbour?.Controller != null )
				{
					neighbour.Controller.OnNeighbourTileControllerChange( this, (Direction)i, previous, controller );
				}
			}
		}

		public T CreateController<T>(TileController t) where T: TileController, new()
		{
			var controller = new T();
			this.AddController( controller );

			return controller;
		}


		public override string ToString()
		{
			UpdateName();
			return $"SPACE {Name}";
		}

	}


}
