using Sandbox.UI;
using Sandbox;
using System.Collections.Generic;

namespace Degg.UI.Elements
{
	public partial class SplashScreen : Panel
	{
		public Panel Inner { get; set; }
		
		public SplashScreen()
		{
			StyleSheet.Load( "/Degg/Ui/Elements/SplashScreen.scss" );
			AddClass( "splash-screen" );
		}
	}
}
