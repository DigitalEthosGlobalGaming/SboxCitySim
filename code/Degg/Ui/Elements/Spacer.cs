using Sandbox.UI;

namespace Degg.UI.Elements
{
	public partial class Spacer : Panel
	{		
		public Spacer()
		{
			StyleSheet.Load( "Degg/Ui/Elements/Spacer.scss" );
			AddClass( "spacer" );
		
		}
	}
}
