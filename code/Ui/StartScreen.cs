using Sandbox.UI;
using Sandbox;
using CitySim;
using System;

namespace GridSystem.Ui
{
	public partial class StartScreen: Panel
	{

		public Panel Base { get; set; }

		public Button StartButton { get; set; }

		public StartScreen()
		{
			SetTemplate( "Ui/StartScreen.html" );
			StyleSheet.Load( "Ui/StartScreen.scss" );
			StyleSheet.Load( "Ui/Styles/button.scss" );
			AddClass("start-screen");
			StartButton.Text =  "Press [Jump (Space)] To Start" ;
			StartButton.AddClass( "button" );
			StartButton.AddEventListener( "onclick", () => { VoteToStart(); } );
		}

		public override void Tick()
		{

			SetClass( "open", Input.Down( InputButton.Score ) );
			if ( Input.Pressed( InputButton.Jump ) )
			{
				VoteToStart();
			}
		}

		public void VoteToStart()
		{
			var pawn = (Pawn)Local.Client.Pawn;
			var opened = pawn.DisabledControls;
			if ( !opened )
			{
				MyGame.VoteToStart();
			}
			
		}

	}
}
