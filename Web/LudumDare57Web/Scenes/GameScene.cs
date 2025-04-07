using LudumDare57Web.Base;
using LudumDare57Web.Entities;
using LudumDare57Web.Interface;
using LudumDare57Web.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LudumDare57Web.Scenes
{
    internal class GameScene : IScene
    {
        // Mangers
        private readonly SceneManager _sceneManager;
        private readonly ParallaxManager _parallaxManager;
        private readonly ContentManager Content;
        private readonly TextRenderer _textRenderer;



        private Enemy _enemy;
        private Tilemap _tilemap;
        private Player _player;

        private bool _tutorial;
        private bool _initialSpaceRelease; // Coming in from Start Scene

        public GameScene(ContentManager ContentManager, SceneManager SceneManager, ParallaxManager ParallaxManager, TextRenderer TextRenderer)
        {
            Content = ContentManager;
            _sceneManager = SceneManager;
            _parallaxManager = ParallaxManager;
            _textRenderer = TextRenderer;

            _tutorial = true;
            _initialSpaceRelease = false;


        }
        public void Load()
        {
            Texture2D mapTiles = Content.Load<Texture2D>("Textures/Tiles");
            _tilemap = new Tilemap(mapTiles);

            Texture2D _playerTexture = Content.Load<Texture2D>("Textures/Hero");
            _player = new Player(_playerTexture, new Vector2(Global.ResX / 2 - Global.Scale * 16 / 2, 0), 16 * Global.Scale, 16 * Global.Scale, _tilemap);

            Texture2D _enemySheet = Content.Load<Texture2D>("Textures/EnemySheet");
            _enemy = new Enemy(_enemySheet, Vector2.Zero, Global.Scale * 16, Global.Scale * 16, _tilemap);
        }
        public void Update(GameTime gameTime)
        {

            if (_tutorial)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space) && _initialSpaceRelease)
                {
                    _tutorial = false;
                    _player.StartInitialDescent();
                }
                else if (!Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    _initialSpaceRelease = true;
                }

            }
            Vector2 movement = _player.Update(gameTime);

            if (movement.X != 0)
            {
                _parallaxManager.IsMoving = true;

                if (movement.X > 0) _parallaxManager.IsMovingForward = true;
                else _parallaxManager.IsMovingForward = false;
            }
            else _parallaxManager.IsMoving = false;

            _parallaxManager.Update();
            _parallaxManager.CurrentLevel = _tilemap.CurrentLevel;

            _tilemap.Update();

            _enemy.Update(gameTime, _player.Rect);

            // Win-Lose Conditions
            if (_player.Rect.Top > Global.ResY && _parallaxManager.CurrentLevel == 0)
            {
                _sceneManager.AddScene(new TitleScene(Content, _sceneManager, _parallaxManager, _textRenderer, false));
            }
            else if (_player.Rect.Bottom < 0 && _parallaxManager.CurrentLevel == 4)
            {
                _sceneManager.AddScene(new TitleScene(Content, _sceneManager, _parallaxManager, _textRenderer, true));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _parallaxManager.Draw(spriteBatch);
            _tilemap.Draw(spriteBatch);
            _player.Draw(spriteBatch);
            _enemy.Draw(spriteBatch);

            if (_tutorial)
            {
                _textRenderer.SetFontScale(2);
                _textRenderer.DrawStringWrapAroundCentered(spriteBatch, "USE WASD TO MOVE. ", new Vector2(0, 100), Global.ResX, Color.White);
                _textRenderer.DrawStringWrapAroundCentered(spriteBatch, "LEFT CLICK WITH MOUSE TO ADD/REMOVE BLOCKS ANYWHERE ON SCREEN.", new Vector2(0, 150), Global.ResX, Color.White);
                _textRenderer.DrawStringWrapAroundCentered(spriteBatch, "YOU CAN USE SHIFT TO NOT TO FALL FROM THE EDGE.", new Vector2(0, 200), Global.ResX, Color.White);
                _textRenderer.DrawStringWrapAroundCentered(spriteBatch, "YOUR GOAL IS TO GO UP. PRESS SPACE TO BEGIN...", new Vector2(0, 250), Global.ResX, Color.White);
                _textRenderer.SetFontScale(3);
                _textRenderer.DrawStringWrapAroundCentered(spriteBatch, "PRESS SPACE TO BEGIN...", new Vector2(0, 300), Global.ResX, Color.White);
            }
        }
    }
}
