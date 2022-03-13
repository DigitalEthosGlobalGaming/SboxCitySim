using Sandbox;
using Sandbox.UI;

namespace CitySim.UI
{
	public class GameControlInfoUI : Panel
	{
		private Panel bgPanel;

		private Image PlaceBtn;
		private Image DiscardBtn;
		private Image ScoreBtn;
		private Image MenuBtn;

		// Configuration for the Input Glyphs.
		private const InputGlyphSize inputGlyphSize = InputGlyphSize.Small;
		private GlyphStyle glyphStyle = GlyphStyle.Dark;

		public GameControlInfoUI()
		{
			StyleSheet.Load( "Ui/GameControlInfoUI.scss" );
			this.AddClass( "container" );

			// Give the layout a separate panel to work on, while the parent of this will manage position.
			bgPanel = AddChild<Panel>( "gamecontrolinfoui_panel" );

			// Create the Button Glyphs
			CreateButtonGlyph( InputButton.Attack1, ref PlaceBtn, "Place");
			CreateButtonGlyph( InputButton.Attack2, ref DiscardBtn, "Discard" );
			CreateButtonGlyph( InputButton.Score, ref ScoreBtn, "Score" );
			CreateButtonGlyph( InputButton.Walk, ref MenuBtn, "Menu" );
		}

		private void CreateButtonGlyph( InputButton _inputBtn, ref Image _element, string _label )
		{
			var texture = Input.GetGlyph( _inputBtn, inputGlyphSize, glyphStyle );

			var placeBtnPanel = bgPanel.AddChild<Panel>();
			placeBtnPanel.AddClass( "button-prompt" );

			_element = placeBtnPanel.AddChild<Image>();
			_element.AddClass( "button-prompt-image" );
			_element.Texture = texture;

			var placeBtnLabel = placeBtnPanel.AddChild<Label>();
			placeBtnLabel.AddClass( "button-prompt-label" );
			placeBtnLabel.Text = _label;
		}
	}
}
