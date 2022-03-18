using Sandbox;
using System;
using System.Collections.Generic;

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

		public static Dictionary<object, bool> CheckPressed()
		{
			var dictionary = new Dictionary<object, bool>();
			foreach ( var item in Enum.GetValues( typeof(InputButton)))
			{
				var input = (InputButton) item;
				if ( Input.Pressed( input ) )
				{
					Log.Info( input );
					dictionary[item] = true;
				}
			}

			return dictionary;
		}
	}
}
