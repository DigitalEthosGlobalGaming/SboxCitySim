
using Sandbox;
using System.Collections.Generic;

namespace GridSystem
{

    public partial class GridSpace: ModelEntity
    {
		[Net]
        public Vector2 GridPosition { get; set; }

		[Net]
		public GridMap Map { get; set; }

		[Net]
		public List<GridItem> Items { get; set; } = new List<GridItem>();



		public Vector3 GetWorldPosition()
		{
			return this.Map.GetWorldSpace( (int)this.GridPosition.x, (int)this.GridPosition.y );
		}

		public virtual float GetMovementWeight(GridSpace a)
		{
			if (a == null)
			{
				return -1;
			}
			return 10;
		}

		public virtual void Tick(float delta)
		{

		}

		public virtual void ClientTick( float delta )
		{

		}
		public virtual void ServerTick( float delta )
		{

		}


		public virtual void OnAddToMap()
		{
			UpdatePosition();
		}

		public void UpdatePosition()
		{

			Position = GetWorldPosition();
		}

		public T GetNeighbour<T>( int x, int y ) where T : GridSpace
		{
			var positionToGet = new Vector2(x,y) + GridPosition;
			return (T)Map.GetSpace( positionToGet );
		}
		public T[] GetNeighbours<T>() where T : GridSpace
		{
			var up = GetNeighbour<T>( 0, -1 );
			var down = GetNeighbour<T>( 0, 1 );
			var left = GetNeighbour<T>( -1, 0 );
			var right = GetNeighbour<T>( 1, 0 );
			T[] neighbours = { up, right, down, left  };
			return neighbours;
		}
		public GridSpace GetNeighbour(Vector2 pos)
		{
			var positionToGet = pos + GridPosition;
			return Map.GetSpace( positionToGet );
		}

		public void AddItem(GridItem item, bool triggerEvents = true)
        {
            item.Space = this;
            Items.Add(item);
            if (triggerEvents)
            {
                this.OnItemAdded(item);
                item.OnAdded();
            }
        }

        public void RemoveItem(GridItem item, bool triggerEvents = true)
        {
            item.Space = null;
            Items.Remove(item);
            if (triggerEvents)
            {
                OnItemRemoved(item);
                item.OnRemove();
            }
        }

        public virtual void OnItemAdded(GridItem item) { }
        public virtual void OnItemRemoved(GridItem item) { }

        public override string ToString()
        {
            return $"SPACE [{GridPosition.x},{GridPosition.y}]";
        }
    }
}
