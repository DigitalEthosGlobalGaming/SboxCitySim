using Sandbox.UI;
using Sandbox;
using Sandbox.UI.Construct;

namespace CitySim.UI
{
	public partial class ScoreboardRow : ScoreboardEntry
	{
		public Label Score;

		public ScoreboardRow()
		{
			AddClass( "entry" );

			Score = Add.Label( "", "score" );
			Ping = Add.Label( "", "ping" );
		}

		RealTimeSince TimeSinceUpdate = 0;

		public override void Tick()
		{
			base.Tick();

			if ( !IsVisible )
				return;

			if ( !Client.IsValid() )
				return;

			if ( TimeSinceUpdate < 0.1f )
				return;

			TimeSinceUpdate = 0;
			UpdateData();
		}

		public override void UpdateData()
		{
			PlayerName.Text = Client.Name;
			Score.Text = Client.GetInt( "score" ).ToString();
			Ping.Text = Client.Ping.ToString();
			SetClass( "me", Client == Local.Client );
		}
	}
}
