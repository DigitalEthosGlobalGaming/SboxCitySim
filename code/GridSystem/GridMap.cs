
using System;
using Sandbox;
using System.Collections.Generic;

namespace GridSystem
{


	// Grid map is an instance of a space. This is the top level "grid" itself.
	public partial class GridMap : Entity
	{
		[Net]
		public List<GridSpace> Grid { get; set; }
		[Net]
		public int XSize { get; set; }
		[Net]
		public int YSize { get; set; }


		[Net]
		public Vector2 GridSize { get; set; }

		[Net]
		public bool IsSetup { get; set; } = false;

		public GridMap( )
		{
			Transmit = TransmitType.Always;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			foreach(var i in Grid)
			{
				i.Delete();
			}
				
		}


		public Vector3 GetWorldSpace( int x, int y )
		{
			var percentages = (new Vector2( x, y )) * GridSize;
			return new Vector3( percentages.x, percentages.y, 0 ) + Position - (GetMapSize() / 2);
		}

		public void Init<T>( Vector3 position, Vector2 gridSize, int xSize, int ySize ) where T : GridSpace, new()
		{
			if ( !IsSetup )
			{
				XSize = xSize;
				YSize = ySize;
				Grid = new List<GridSpace>( xSize * ySize );
				Position = position;
				GridSize = gridSize;
				for ( int x = 0; x < XSize; x++ )
				{
					for ( int y = 0; y < YSize; y++ )
					{
						var newSpace = new T();
						newSpace.Map = this;
						newSpace.GridPosition = new Vector2( x, y );
						Grid.Add( newSpace );
						newSpace.SetParent( this );
						newSpace.OnAddToMap();
					}
				}
			}
			IsSetup = true;
		}

		public int TransformGridPosition(int x, int y)		{
			return (x * XSize) + y;
		}
		public int TransformGridPosition( float x, float y )
		{
			return ((int)x * XSize) + ((int)y);
		}



		public GridSpace GetSpace( Vector2 position )
		{
			return GetSpace( position.x, position.y );
		}

		public Vector2 GetGridSize()
		{
			return this.GridSize;
		}
		public Vector3 GetMapSize()
		{
			return new Vector3(this.GridSize * new Vector2(XSize, YSize));
		}

		public GridSpace GetSpace( float x, float y )
		{
			return GetSpace( (int)x, (int)y );
		}

		public GridSpace GetSpace( int x, int y )
		{
			if ( Grid == null)
			{
				return null;
			}
			if ( (x < XSize && x >= 0) && (y < YSize && y >= 0) )
			{
				var amount = TransformGridPosition( x, y );
				if (amount > 0 && amount < Grid.Count)
				{
					return Grid[amount];
				}
				
			}

			return null;
		}


		public List<GridSpace> CreatePath(Vector2 start, Vector2 end )
		{
			var mesh =  new NavMesh( this );
			return mesh.BuildPath( start, end );
		}
		public GridSpace GetRandomSpace()
		{
			var rnd = new Random();
			var x = rnd.Next( 0, XSize-1 );
			var y = rnd.Next( 0, YSize-1 );

			return GetSpace( (int)x, (int)y );
		}

		public bool MoveItem( GridItem item, Vector2 newPosition )
		{
			var oldSpace = item.Space;
			var newSpace = GetSpace( newPosition );
			if ( newSpace == null )
			{
				return false;
			}
			oldSpace.RemoveItem( item, false );
			newSpace.AddItem( item, false );

			item.OnMove( newPosition, oldSpace.Position );

			return true;
		}

		public bool AddItem( GridItem item, Vector2 newPosition )
		{
			if ( item.Space != null )
			{
				return MoveItem( item, newPosition );
			}

			var newSpace = GetSpace( newPosition );
			if ( newSpace == null )
			{
				return false;
			}

			newSpace.AddItem( item );
			return true;
		}

		public List<GridItem> GetItems( Vector2 position )
		{
			var space = GetSpace( position );
			if ( space == null )
			{
				return new List<GridItem>();
			}
			else
			{
				return space.Items;
			}
		}


		public void ClientTick()
		{
			foreach(var item in Grid)
			{
				if ( item != null )
				{
					item.ClientTick( Time.Delta );
				}
			}
		}
		public void ServerTick()
		{
			foreach ( var item in Grid )
			{
				item.ServerTick( Time.Delta );
			}
		}

	}
}
