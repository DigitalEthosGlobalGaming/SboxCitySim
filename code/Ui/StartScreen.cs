using Sandbox.UI;
using Sandbox;
using CitySim;
using System;

namespace GridSystem.Ui
{
	public partial class StartScreen: Panel
	{

		public Panel Base { get; set; }

		public Button StartNormalButton { get; set; }
		public Button StartChaosButton { get; set; }

		public StartScreen()
		{
			SetTemplate( "Ui/StartScreen.html" );
			StyleSheet.Load( "Ui/StartScreen.scss" );
			StyleSheet.Load( "Ui/Styles/button.scss" );
			AddClass("start-screen");
			StartNormalButton.Text =  "Play" ;
			StartNormalButton.AddClass( "button" );
			StartNormalButton.AddEventListener( "onclick", () => { VoteToStart(MyGame.GameModes.Normal); } );

			StartChaosButton.Text = "Play Chaos Mode";
			StartChaosButton.AddClass( "button" );
			StartChaosButton.AddEventListener( "onclick", () => { VoteToStart( MyGame.GameModes.Chaos); } );
		}

		public override void Tick()
		{

			SetClass( "open", Input.Down( InputButton.Score ) );
		}

		public void VoteToStart(MyGame.GameModes mode)
		{
			var pawn = (Pawn)Local.Client.Pawn;
			var opened = pawn.DisabledControls;
			if ( !opened )
			{
				MyGame.VoteToStart( mode );
			}
			
		}

	}
}
