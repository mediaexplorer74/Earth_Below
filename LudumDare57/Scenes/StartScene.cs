using LudumDare57.Base;
using LudumDare57.Interface;
using LudumDare57.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LudumDare57.Scenes
{
    internal class StartScene : IScene
    {
        ContentManager Content;
        private SceneManager _sceneManager;
        private ParallaxManager _parallaxManager;

        private TextRenderer _textRenderer;
        public StartScene(ContentManager ContentManager, SceneManager SceneManager, ParallaxManager ParallaxManager)
        {
            Content = ContentManager;
            _sceneManager = SceneManager;
            _parallaxManager = ParallaxManager;
        }

        public void Load()
        {
            Texture2D _font = Content.Load<Texture2D>("Textures/FontAtlas");
            _textRenderer = new TextRenderer(_font);
            _textRenderer.SetAsciiRange(32, 151);
            _textRenderer.SetFontWidthInPixels(8);
            _textRenderer.CondenseLetterSpacing(2);
            _textRenderer.SetFontScale(10);
        }

        private KeyboardState _previousKeyboardState;
        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
            {
                _sceneManager.AddScene(new GameScene(Content, _sceneManager, _parallaxManager, _textRenderer));
            }

            _parallaxManager.Update();

            _previousKeyboardState = keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _parallaxManager.Draw(spriteBatch);
            _textRenderer.SetFontScale(10);
            _textRenderer.DrawString(spriteBatch, "EARTH BELOW", new Vector2(Global.ResX / 2 - _textRenderer.MeasureString("EARTH BELOW").Width / 2, 100));
            _textRenderer.SetFontScale(4);
            _textRenderer.DrawString(spriteBatch, "PRESS SPACE TO START", new Vector2(Global.ResX / 2 - _textRenderer.MeasureString("PRESS SPACE TO START").Width / 2, Global.ResY - 200));
            _textRenderer.SetFontScale(2);
            _textRenderer.DrawString(spriteBatch, "PRESS F FOR FULLSCREEN", new Vector2(Global.ResX / 2 - _textRenderer.MeasureString("PRESS F FOR FULLSCREEN").Width / 2, Global.ResY - 125));
        }
    }
}
