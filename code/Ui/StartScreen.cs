using Degg.UI;
using Degg.UI.Elements;
using Degg.UI.Forms;
using Degg.UI.Forms.Elements;
using Degg.Util;
using Sandbox;
using Sandbox.UI;

namespace CitySim.UI
{
	public partial class StartScreen: Panel
	{

		public Degg.UI.Forms.Form Form { get; set; }

		public StartScreen()
		{
			var start = AddChild<SplashScreen>();
			var form = start.AddChild<Degg.UI.Forms.Form>();

			form.AddChild<Center>().AddChild<Header>().SetText( "City Sim" );

			form.AddChild<Center>().AddChild<Header>().SetText( "Select Mode", 4 );

			var btn = form.AddChild<FEButton>();
			btn.Label.Text = "Normal";
			btn.SetCenter( true );
			btn.AddEventListener( "onpress", () => { VoteToStart( MyGame.GameModes.Normal ); } );

			btn = form.AddChild<FEButton>();
			btn.Label.Text = "Chaos";
			btn.SetCenter( true );
			btn.AddEventListener( "onpress", () => { VoteToStart( MyGame.GameModes.Chaos ); } );

			btn = form.AddChild<FEButton>();
			btn.Label.Text = "Sandbox";
			btn.SetCenter( true );
			btn.AddEventListener( "onpress", () => { VoteToStart( MyGame.GameModes.Sandbox ); } );

			form.AddChild<Spacer>();
			form.AddChild<Spacer>();

			var button = form.AddChild<Center>().AddChild<ButtonGlyph>();
			button.SetIcon( AdvInput.InputButton( InputButton.Flashlight, InputButton.Menu ), "Show Help" );
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
