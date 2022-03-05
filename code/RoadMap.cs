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

			Score = CalculateScore();
			CheckGameEnd();

		}


		public float CalculateScore()
		{
			var score = 0;
			var totalBusinesses = 0;
			var totalHouses = 0;
			foreach(var s in Grid)
			{
				var tile = (RoadTile)s;
				var neighbours = tile.GetNeighbours<RoadTile>();
				if (tile.TileType == RoadTile.TileTypeEnum.House)
				{
					totalHouses += 1;
					score = score + 5;
					foreach ( var n in neighbours )
					{
						if (n != null)
						{
							if ( n.TileType == RoadTile.TileTypeEnum.Business )
							{
								score = score - 10;
							}
							else if ( n.TileType == RoadTile.TileTypeEnum.Park )
							{
								score = score + 10;
							}
						}
					}
				} else if (tile.TileType == RoadTile.TileTypeEnum.Business)
				{
					totalBusinesses += 1;

					foreach ( var n in neighbours )
					{
						if ( n != null )
						{
							if ( n.TileType == RoadTile.TileTypeEnum.Business )
							{
								score = score + 1;
							}
							else if ( n.TileType == RoadTile.TileTypeEnum.Park )
							{
								score = score - 1;
							}
						}
					}
				}
			}

			return score;
		}

		public void Init()
		{
			Init<RoadTile>( new Vector3( 0, 0, 1000 ), new Vector2( 200, 200 ), 5, 5 );
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
			Log.Info( "New pieces please" );
			foreach(var player in Player.All)
			{
				if (player is Pawn)
				{
					var p = player as Pawn;
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

		public override void ServerTick()
		{
			base.ServerTick();

			if ( IsSetup )
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
