using Sandbox;

namespace CitySim
{
	public class AmbulanceEntityMovement : MovementEntity
	{
		private Sound Siren;

		public AmbulanceEntityMovement()
		{
			IsEmergencyVehicle = true;
			ShouldPullOver = false;

			Siren = PlaySound( "ambulance.siren" );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			Siren.Stop();
		}
	}
}
