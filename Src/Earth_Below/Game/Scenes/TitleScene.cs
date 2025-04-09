using GameManager.Base;
using GameManager.Interface;
using GameManager.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameManager.Scenes
{
    internal class TitleScene : IScene
    {
        ContentManager Content;
        private SceneManager _sceneManager;
        private ParallaxManager _parallaxManager;

        private TextRenderer _textRenderer;

        private bool _win;
        public TitleScene(ContentManager ContentManager, SceneManager SceneManager, 
            ParallaxManager ParallaxManager, TextRenderer textRenderer, bool Win)
        {
            Content = ContentManager;
            _sceneManager = SceneManager;
            _parallaxManager = ParallaxManager;
            _win = Win;
            _textRenderer = textRenderer;
        }

        public void Load() { }

        public void Update(GameTime gameTime)
        {
            _parallaxManager.Update();
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                _sceneManager.RemoveScene(2);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _parallaxManager.Draw(spriteBatch);
            _textRenderer.SetFontScale(2);
            _textRenderer.DrawStringWrapAroundCentered(spriteBatch, _win 
                ? "YOU FEEL WEIGHTLESS AS YOUR BODY SLOWLY ASCENDS TOWARDS THE HEAVENS..."
                : "THE ECHO OF YOUR VOICE SLOWLY DIES OUT AS YOU FELL THE EARTH BELOW...",
                new Vector2(0, 100), Glob.ResX, Color.White);
            _textRenderer.SetFontScale(4);
            _textRenderer.DrawStringWrapAroundCentered(spriteBatch, "PRESS SPACE TO RESTART", 
                new Vector2(0, Glob.ResY - 300), Glob.ResX, Color.White);
        }
    }
}
