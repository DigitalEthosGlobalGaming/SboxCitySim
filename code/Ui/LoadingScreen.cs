using Sandbox.UI;

namespace CitySim.UI
{
	public partial class LoadingScreen : Panel
	{

		public Panel Base { get; set; }

		public LoadingScreen()
		{

			SetTemplate( "Ui/LoadingScreen.html" );
			StyleSheet.Load( "Ui/LoadingScreen.scss" );
		}

		public override void Tick()
		{
			
		}

	}
}
