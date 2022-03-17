using Sandbox;

namespace Degg.Util
{
	public partial class AdvInput
	{
		public static InputButton InputButton( InputButton pc, InputButton controller)
		{
			if (Input.UsingController)
			{
				return controller;
			}
			return pc;
		}

		public static bool Pressed( InputButton pc, InputButton controller )
		{
			if ( Input.UsingController )
			{
				return Input.Pressed(controller);
			}
			return Input.Pressed( pc );
		}
	}
}
