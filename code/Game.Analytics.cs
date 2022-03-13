﻿
using Degg.Degg.Analytics;
using GridSystem.Ui;
using Sandbox;

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
				DeggBackend.Initialise( "5c6bcb5402204249437fb5a7a80a4959", "16813a12f718bc5c620f56944e1abc3ea13ccbac" );
			}
		} 
		
	}
}
