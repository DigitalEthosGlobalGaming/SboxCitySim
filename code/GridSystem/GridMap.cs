
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

		public int CurrentX { get; set; }
		public int CurrentY { get; set; }

		public bool IsStarting { get; set; }

		public Queue<Func<bool>> SetupFunctions { get; set; }

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
				if ( IsServer )
				{
					i.Delete();
				}
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
				SetupFunctions = new Queue<Func<bool>>();
				for ( int x = 0; x < XSize; x++ )
				{
					for ( int y = 0; y < YSize; y++ )
					{
						var tempX = x;
						var tempY = y;
						SetupFunctions.Enqueue( () => CreateNextTile<T>( tempX, tempY ) );
					}
				}
			}
		}

		public bool CreateNextTile<T>(int x, int y) where T : GridSpace, new()
		{
			try
			{
				var newSpace = new T();
				newSpace.Map = this;
				newSpace.GridPosition = new Vector2( x, y );
				Grid.Add( newSpace );
				newSpace.SetParent( this );
				OnSpaceSetup( newSpace );
				newSpace.OnAddToMap();
				return true;
			} catch(Exception e)
			{
				Log.Info( e );
				return false;
			}
			
		}

		public void CheckFinish()
		{
			IsStarting = true;
		}

		public int TransformGridPosition(int x, int y)		{
			return (x * YSize) + y;
		}
		public int TransformGridPosition( float x, float y )
		{
			return ((int)x * XSize) + ((int)y);
		}

		public List<GridSpace> GetGridAsList()
		{
			var grid = new List<GridSpace>();

			foreach(var item in Grid)
			{
				grid.Add( item );
			}
			return grid;
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

		public bool IsPath( Vector2 start, Vector2 end )
		{
			var mesh = new NavMesh( this );
			return mesh.BuildPath( start, end ).Count > 0;
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

		public virtual void OnSpaceSetup(GridSpace space)
		{

		}

		public virtual void OnSetup()
		{

		}

		


		public virtual void ClientTick()
		{
			if ( IsSetup )
			{
				foreach ( var item in Grid )
				{
					if ( item != null )
					{
						item.ClientTick( Time.Delta );
					}
				}
			}
		}

		public virtual void ServerTick()
		{
			var startTime = Time.Now;
			var amountToCreateAtOnce = 10;

			if ( IsSetup == false )
			{
				while ( amountToCreateAtOnce > 0 && !IsSetup )
				{
					amountToCreateAtOnce = amountToCreateAtOnce - 1;
					if ( SetupFunctions.Count > 0 )
					{
						var callBack = SetupFunctions.Dequeue();
						if ( callBack != null )
						{
							callBack();
						}
					}
					else
					{
						IsSetup = true;
						OnSetup();
					}
				}
			}
		}

	}
}
