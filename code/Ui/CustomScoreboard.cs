using Sandbox.UI;
using Sandbox;
using Sandbox.UI.Construct;

namespace GridSystem.Ui
{
	public partial class CustomScoreboard : Scoreboard<ScoreboardRow>
	{
		public CustomScoreboard()
		{
			StyleSheet.Load( "Ui/CustomScoreboard.scss" );
			AddClass( "custom-scoreboard" );
			RemoveClass( "scoreboard" );

			AddHeader();
			Canvas = Add.Panel( "canvas" );
		}


		protected override void AddHeader()
		{
			Header = Add.Panel( "header" );
			Header.Add.Label( "Name", "name" );
			Header.Add.Label( "Score", "score" );
			Header.Add.Label( "Ping", "ping" );
		}
	}
}
