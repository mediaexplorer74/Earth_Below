using LudumDare57Web.Managers;
using LudumDare57Web.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace LudumDare57Web
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class LudumDare57WebGame : Game
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
        public LudumDare57WebGame()
        {
            _graphics = new GraphicsDeviceManager(this);

            Global.ResX = 1280;
            Global.ResY = 720;
            Global.Scale = 5;
            Global.TextureSize = 8;

            _graphics.PreferredBackBufferWidth = Global.ResX;
            _graphics.PreferredBackBufferHeight = Global.ResY;

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

            Global.RenderTargetWidth = _renderTargetDestination.Width;
            Global.RenderTargetHeight = _renderTargetDestination.Height;

            Global.OffsetX = _renderTargetDestination.X;
            Global.OffsetY = _renderTargetDestination.Y;
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            CalculateRenderTargetDestination();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
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

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
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

            /*if (_sceneManager.GetCurrentScene() is StartScene && keyboardState.IsKeyDown(Keys.F) && !_previousKeyboardState.IsKeyDown(Keys.F))
            {
                _graphics.IsFullScreen = !_graphics.IsFullScreen;
                _graphics.ApplyChanges();
            }*/
            _previousKeyboardState = keyboardState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
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
