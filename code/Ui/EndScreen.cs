using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace CitySim.UI
{
	public partial class EndScreen : Panel
	{

		public Panel Base { get; set; }

		public Panel PlayerScores { get; set; }
		public Panel YourScore { get; set; }
		public Label CurrentPlayerLabel { get; set; }

		Dictionary<IClient, Panel> Rows = new();

		PriorityQueue<IClient, int> ClientScores = new();

		public EndScreen()
		{
			SetTemplate( "Ui/EndScreen.html" );
			StyleSheet.Load( "Ui/EndScreen.scss" );
			CreateScoreboard();
		}

		public void CreateScoreboard()
		{
			if ( PlayerScores != null )
			{
				PlayerScores.DeleteChildren();
				foreach ( var client in Game.Clients.Except( Rows.Keys ) )
				{
					ClientScores.Enqueue( client, client.GetInt( "score", 0 ) );
				}

				var topSpotsToShow = 3;
				for ( int i = 0; i < topSpotsToShow; i++ )
				{

					if ( ClientScores.Count > 0 )
					{
						var nextClient = ClientScores.Dequeue();
						if ( nextClient != null )
						{
							var p = PlayerScores.AddChild<Panel>();

							p.AddClass( "score-row" );

							var labelName = p.AddChild<Label>();
							labelName.AddClass( "name" );

							var label = $"{nextClient.Name}: {nextClient.GetInt( "score", 0 )}";
							labelName.Text = label;
						}
					}
				}


				var currentPlayer = Game.LocalClient;
				CurrentPlayerLabel.Text = currentPlayer.GetInt( "score", 0 ).ToString();

			}

		}

		public override void Tick()
		{

		}

	}
}
