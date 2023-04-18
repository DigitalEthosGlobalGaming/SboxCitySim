using Degg.GridSystem;
using Sandbox;
using System.Collections.Generic;

namespace CitySim
{
	public partial class RoadMap : GridMap
	{
		public const float TileScale = 0.25f;

		[Net]
		public bool IsEnd { get; set; }

		public float TimeBetweenPieces { get; set; } = 3;
		public float TimeBetweenPiecesModifier { get; set; } = 1;

		[Net]
		public float TimeForNewPiece { get; set; }
		[Net]
		public float Score { get; set; }

		public void UpdateScore()
		{
			CheckGameEnd();
		}



		public int CalculateTileScore( GenericTile tile )
		{
			var score = 1;

			var neighbours = tile.GetNeighbours<GenericTile>();
			var a = tile;

			foreach ( var b in neighbours )
			{
				score += a.GetTileScore( b );
			}

			if ( score < 0 )
			{
				score = 0;
			}

			return score;
		}

		public void Init( int xAmount, int yAmount )
		{
			Init<GenericTile>( new Vector3( 0, 0, 250 ), new Vector2( TileScale * 200.0f, TileScale * 200.0f ), xAmount, yAmount );
		}

		public override void OnSpaceSetup( GridSpace space )
		{

		}

		public override void OnSetup()
		{
			var numberOfRows = Game.Random.Int( 1, 4 );
			var numberOfCols = Game.Random.Int( 1, 4 );
			var startX = 0;
			var startY = 0;
			var xStart = XSize - startX - 1;
			var yStart = YSize - startY - 1;
			IsEnd = false;

			for ( int row = 0; row < numberOfRows; row++ )
			{

				var xPosition = Game.Random.Int( startX, xStart );
				for ( int i = 0; i < XSize; i++ )
				{
					var space = (GenericTile)GetSpace( i, xPosition );
					if ( space != null )
					{
						space.CreateController<RoadTileController>();
					}
				}
			}

			for ( int col = 0; col < numberOfCols; col++ )
			{
				var yPosition = Game.Random.Int( startY, yStart );
				for ( int i = 0; i < YSize; i++ )
				{
					var space = (GenericTile)GetSpace( yPosition, i );
					if ( space != null )
					{
						space.CreateController<RoadTileController>();
					}
				}
			}


			Event.Run( "citysim.map_setup", this );
		}

		public int GetBlankTiles()
		{
			var amount = 0;
			foreach ( var s in Grid )
			{
				var tile = (GenericTile)s;
				if ( tile.GetTileType() == GenericTile.TileTypeEnum.Base )
				{
					amount = amount + 1;
				}
			}

			return amount;
		}

		public void CheckGameEnd()
		{
			var totalTiles = Grid.Count;
			var tilesToEnd = totalTiles - (totalTiles * 0.9);

			var blank = GetBlankTiles();
			if ( blank <= tilesToEnd )
			{
				MyGame.GameObject.SetGameState( MyGame.GameStateEnum.End );
			}
		}


		public void GivePlayersNewPiece()
		{
			if ( MyGame.GameState != MyGame.GameStateEnum.Playing )
			{
				return;
			}
			foreach ( var client in Game.Clients )
			{
				var player = client.Pawn;
				if ( player is Pawn )
				{
					var p = player as Pawn;
					if ( p.SelectedTileType == GenericTile.TileTypeEnum.Base )
					{
						p.SelectNextRandomTile();
					}
				}
			}
		}

		public override void ServerTick()
		{
			base.ServerTick();

			if ( MyGame.GameState == MyGame.GameStateEnum.Playing )
			{
				if ( TimeBetweenPieces > 0 && TimeForNewPiece < Time.Now )
				{
					TimeForNewPiece = Time.Now + TimeBetweenPieces + (Game.Clients.Count * TimeBetweenPiecesModifier);
					GivePlayersNewPiece();
				}
			}
		}

		public List<GenericTile> GetGenericTiles()
		{
			// Conversion is expensive...
			List<GenericTile> tiles = new List<GenericTile>( this.Grid.Count );
			for ( int i = 0; i < this.Grid.Count; i++ )
			{
				tiles.Add( (GenericTile)this.Grid[i] );
			}
			return tiles;
		}

	}

}
