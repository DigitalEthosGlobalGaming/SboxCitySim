using Sandbox.UI;
using Sandbox;
using CitySim;
using System;

namespace GridSystem.Ui
{
	public partial class EndScreen: Panel
	{

		public Panel Base { get; set; }

		public EndScreen()
		{

			SetTemplate( "Ui/EndScreen.html" );
			StyleSheet.Load( "Ui/EndScreen.scss" );
		}

		public override void Tick()
		{
			
		}

	}
}
