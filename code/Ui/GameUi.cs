using Sandbox;
using Sandbox.UI;


namespace GridSystem.Ui
{
	public partial class GameUi: HudEntity<RootPanel>
	{

		public GameUi()
		{
			RootPanel.AddChild<Main>();
			RootPanel.AddChild<ChatBox>();


		}
	}
}
