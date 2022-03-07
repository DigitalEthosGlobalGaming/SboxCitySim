﻿using GridSystem;
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



		public int CalculateTileScore(GenericTile tile)
		{
			var score = 1;

			var neighbours = tile.GetNeighbours<GenericTile>();
			var a = tile;

			foreach ( var b in neighbours )
			{
				score += a.GetTileScore( b );
			}

			if ( score  < 0)
			{
				score = 0;
			}

			return score;
		}

		public void Init(int xAmount, int yAmount)
		{
			Init<GenericTile>( new Vector3( 0, 0, 1000 ), new Vector2( 200, 200 ), xAmount, yAmount );
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
					var space = (GenericTile)GetSpace( i, xPosition );
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
					var space = (GenericTile)GetSpace( yPosition, i );
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
				var tile = (GenericTile)s;
				if ( tile.TileType == GenericTile.TileTypeEnum.Base )
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
