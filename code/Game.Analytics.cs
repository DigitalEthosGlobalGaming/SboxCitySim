
using Degg.Analytics;
using Degg.Util;
using Sandbox;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace CitySim
{


	public partial class MyGame : GameManager
	{
		public void SetupAnalytics()
		{
			if ( Game.IsServer )
			{
				GameAnalytics.ConfigureBuild( "1.0.0" );
				GameAnalytics.Initialise( "5c6bcb5402204249437fb5a7a80a4959", "16813a12f718bc5c620f56944e1abc3ea13ccbac" );

			}
		}

		[ConCmd.Server( "degg.backend.send_events" )]
		public static void UpdateDeggBackendEvents()
		{
			AdvLog.Info( "Test" );
			Degg.Backend.DeggBackend.SendEvents();
		}

	}
}
