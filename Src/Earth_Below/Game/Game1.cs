using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameManager.Managers;
using GameManager.Scenes;
using System;

namespace GameManager
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Managers
        private ParallaxManager _parallaxManager;
        private SceneManager _sceneManager;

        private AudioManager _audioManager;


        // Independent Screen Resolution Rendering
        private RenderTarget2D _renderTarget;
        private Rectangle _renderTargetDestination;

        private bool _isResizing;

        private int NATIVE_SCREEN_WIDTH = 1280;
        private int NATIVE_SCREEN_HEIGHT = 720;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            Glob.ResX = 1280;
            Glob.ResY = 720;
            Glob.Scale = 5;
            Glob.TextureSize = 8;

            _graphics.PreferredBackBufferWidth = Glob.ResX;
            _graphics.PreferredBackBufferHeight = Glob.ResY;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.HardwareModeSwitch = false; // Borderless Fullscreen

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnClientSizeChanged;

            _audioManager = new AudioManager(Content);
            if (!_audioManager.IsPlaying())
                _audioManager.Start();


        }

        private void OnClientSizeChanged(Object sender, EventArgs e)
        {
            if (!_isResizing && Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
            {
                _isResizing = true;
                CalculateRenderTargetDestination();
                _isResizing = false;
            }
        }

        private void CalculateRenderTargetDestination()
        {
            Point size = GraphicsDevice.Viewport.Bounds.Size;

            float scaleX = (float)size.X / _renderTarget.Width;
            float scaleY = (float)size.Y / _renderTarget.Height;
            float scale = Math.Min(scaleX, scaleY);

            _renderTargetDestination.Width = (int)(_renderTarget.Width * scale);
            _renderTargetDestination.Height = (int)(_renderTarget.Height * scale);

            _renderTargetDestination.X = (size.X - _renderTargetDestination.Width) / 2;
            _renderTargetDestination.Y = (size.Y - _renderTargetDestination.Height) / 2;

            Glob.RenderTargetWidth = _renderTargetDestination.Width;
            Glob.RenderTargetHeight = _renderTargetDestination.Height;

            Glob.OffsetX = _renderTargetDestination.X;
            Glob.OffsetY = _renderTargetDestination.Y;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            CalculateRenderTargetDestination();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _renderTarget = new RenderTarget2D(GraphicsDevice, NATIVE_SCREEN_WIDTH, NATIVE_SCREEN_HEIGHT);
            _parallaxManager = new(Content)
            {
                IsMoving = false
            };

            _sceneManager = new SceneManager();
            _sceneManager.AddScene(new StartScene(Content, _sceneManager, _parallaxManager));
        }

        private KeyboardState _previousKeyboardState;
        protected override void Update(GameTime gameTime)
        {
#if DEBUG
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
#endif
            /*if (Keyboard.GetState().IsKeyDown(Keys.Space))
                _parallaxManager.IsMoving = true;
            else _parallaxManager.IsMoving = false;*/

            // TODO: Add your update logic here

            //_parallaxManager.Update();

            KeyboardState keyboardState = Keyboard.GetState();
            _sceneManager.GetCurrentScene().Update(gameTime);
            base.Update(gameTime);

            if (_sceneManager.GetCurrentScene() is StartScene && keyboardState.IsKeyDown(Keys.F) && !_previousKeyboardState.IsKeyDown(Keys.F))
            {
                _graphics.IsFullScreen = !_graphics.IsFullScreen;
                _graphics.ApplyChanges();
            }
            _previousKeyboardState = keyboardState;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            GraphicsDevice.SetRenderTarget(_renderTarget);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            //_parallaxManager.Draw(_spriteBatch);
            _sceneManager.GetCurrentScene().Draw(_spriteBatch);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            //_parallaxManager.Draw(_spriteBatch);
            _spriteBatch.Draw(_renderTarget, _renderTargetDestination, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
