
using Degg.Analytics;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace CitySim
{


	public partial class MyGame : Sandbox.Game
	{
		public void SetupAnalytics()
		{
			if ( IsServer )
			{
				GameAnalytics.ConfigureBuild( "1.0.0" );
				GameAnalytics.Initialise( "5c6bcb5402204249437fb5a7a80a4959", "16813a12f718bc5c620f56944e1abc3ea13ccbac" );

			}
		} 
		
	}
}
