using GridSystem;
using Sandbox;
using System;

namespace CitySim
{
	public partial class RoadMap : GridMap
	{

		[Net]
		public bool IsEnd { get; set; }

		public float TimeBetweenPieces { get; set; } = 3;

		[Net]
		public float TimeForNewPiece { get; set; }
		[Net]
		public float Score { get; set;}

		public void UpdateScore() {
			CheckGameEnd();
		}

		public static bool IsCombination(RoadTile aTile, RoadTile bTile, RoadTile.TileTypeEnum a, RoadTile.TileTypeEnum b)
		{
			if (aTile.TileType == a && bTile.TileType == b)
			{
				return true;
			}
			if ( bTile.TileType == a && aTile.TileType == b )
			{
				return true;
			}

			return false;
		}


		public int CalculateTileScore(RoadTile tile)
		{
			var score = 1;

			var neighbours = tile.GetNeighbours<RoadTile>();
			var a = tile;

			foreach ( var b in neighbours )
			{
				if ( b != null )
				{
					// House House
					if ( IsCombination( a, b, RoadTile.TileTypeEnum.House, RoadTile.TileTypeEnum.House ) )
					{
						score = score + 1;
					// House Park
					} else if ( IsCombination( a, b, RoadTile.TileTypeEnum.House, RoadTile.TileTypeEnum.Park ) )
					{
						score = score + 2;
					}
					// House Business
					else if ( IsCombination( a, b, RoadTile.TileTypeEnum.House, RoadTile.TileTypeEnum.Business ) )
					{
						score = score - 1;
					}
					// Park Park
					else if ( IsCombination( a, b, RoadTile.TileTypeEnum.Park, RoadTile.TileTypeEnum.Park ) )
					{
						score = score + 2;
					}
					// Park Road
					else if ( IsCombination( a, b, RoadTile.TileTypeEnum.Park, RoadTile.TileTypeEnum.Road ) )
					{
						score = score - 1;
					}
					// Park Business
					else if ( IsCombination( a, b, RoadTile.TileTypeEnum.Park, RoadTile.TileTypeEnum.Business ) )
					{
						score = score - 1;
					}					
					// Business Business
					else if ( IsCombination( a, b, RoadTile.TileTypeEnum.Business, RoadTile.TileTypeEnum.Business ) )
					{
						score = score + 2;
					}
				}
			}

			if ( score  < 0)
			{
				score = 0;
			}

			return score;
		}

		public void Init(int xAmount, int yAmount)
		{
			Init<RoadTile>( new Vector3( 0, 0, 1000 ), new Vector2( 200, 200 ), xAmount, yAmount );
		}

		public override void OnSpaceSetup(GridSpace space)
		{
			
		}

		public override void OnSetup()
		{
			var numberOfRows = Rand.Int( 1, 4 );
			var numberOfCols = Rand.Int( 1, 4 );
			var startX = 0;
			var startY = 0;
			var xStart = XSize - startX - 1;
			var yStart = YSize - startY - 1;
			IsEnd = false;

			for ( int row = 0; row < numberOfRows; row++ )
			{

				var xPosition = Rand.Int( startX, xStart );
				for ( int i = 0; i < XSize; i++ )
				{
					var space = (RoadTile)GetSpace( i, xPosition );
					if ( space != null )
					{
						space.SetHasRoad( true );
					}
				}
			}

			for ( int col = 0; col < numberOfCols; col++ )
			{
				var yPosition = Rand.Int( startY, yStart );
				for ( int i = 0; i < YSize; i++ )
				{
					var space = (RoadTile)GetSpace( yPosition, i );
					if ( space != null )
					{
						space.SetHasRoad( true );
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
				var tile = (RoadTile)s;
				if ( tile.TileType == RoadTile.TileTypeEnum.Base )
				{
					amount = amount + 1;
				}
			}

			return amount;
		}

		public void CheckGameEnd()
		{
			if ( GetBlankTiles() == 0)
			{
				IsEnd = true;
			}
		}


		public void GivePlayersNewPiece()
		{
			if (MyGame.GameState != MyGame.GameStateEnum.Playing)
			{
				return;
			}
			foreach(var client in Client.All)
			{
				var player = client.Pawn;
				if (player is Pawn)
				{
					var p = player as Pawn;
					if ( p.SelectedTileType == RoadTile.TileTypeEnum.Base )
					{
						var start = 1;
						var end = 6;
						var rndInt = Rand.Int( start, end );
						if ( rndInt > 4 )
						{
							p.SelectedTileType = RoadTile.TileTypeEnum.House;
						}
						else
						{
							p.SelectedTileType = (RoadTile.TileTypeEnum)Enum.GetValues( typeof( RoadTile.TileTypeEnum ) ).GetValue( rndInt );
						}
					}
				}
			}
		}

		public override void ServerTick()
		{
			base.ServerTick();

			if (MyGame.GameState == MyGame.GameStateEnum.Playing)
			{
				if ( TimeForNewPiece < Time.Now )
				{
					TimeForNewPiece = Time.Now + TimeBetweenPieces + Client.All.Count;
					GivePlayersNewPiece();
				}
			}
		}
	}

}
