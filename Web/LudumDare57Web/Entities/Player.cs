using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using LudumDare57Web.Base;
using LudumDare57Web.Interface;

namespace LudumDare57Web.Entities
{
    internal class Player : Sprite
    {
        private Rectangle _sourceRectangle;
        private const int TEXTURE_WIDTH = 16;

        private bool _tutorial = true;
        public bool Tutorial
        {
            get { return _tutorial; }
            set { _tutorial = value; }
        }
        private bool _initialDescent = false;
        public bool InitialDescent
        {
            get { return _initialDescent; }
            set { _initialDescent = value; }
        }

        public void StartInitialDescent()
        {
            _tutorial = false;
            _initialDescent = true;
        }

        private float _animationElapsedTime;
        private const float ANIMATION_TIME = .15f;
        private const int ANIMATION_FRAMES_COUNT = 2;

        private float _coyoteTimerElapsedTime;
        private const float COYOTE_TIME = .15f;

        // For collision detection
        readonly int TEXTURE_OFFSET_X = 6 * Global.Scale;
        readonly int TEXTURE_OFFSET_Y = 3 * Global.Scale;
        readonly int HITBOX_WIDTH = Global.Scale * 3;
        readonly int HITBOX_HEIGHT = Global.Scale * 11;
        private readonly Tilemap _map;
        private Rectangle _hitbox;
        private Point _bottomColliderLeftmostPoint, _bottomColliderRightmostPoint;

        private float _speed = Global.Scale;

        // Gravity stuff
        private const float TERMINAL_VELOCITY = 25f;
        private const float G_FORCE = .75f;
        private const float JUMPING_POWER = 15f;
        private float _yAxisVelocity;
        private bool _isPlayerGrounded = false;

        private bool _isClinging;
        private int _clingingOffset;
        private bool _isFacingRight = false;

        /*public bool IsPlayerGrounded
        {
            get { return _isPlayerGrounded; }
            set { _isPlayerGrounded = value; }
        }*/

        public Player(Texture2D Texture, Vector2 Position, int Width, int Height, Tilemap Map) : base(Texture, Position, Width, Height)
        {
            _map = Map;
            _sourceRectangle = new Rectangle(0, 0, TEXTURE_WIDTH, TEXTURE_WIDTH);

            _hitbox = CalculatePlayerHitbox();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Rect, _sourceRectangle, Color.White);
            //spriteBatch.Draw(Texture, _hitbox, new Rectangle(7, 4, 1, 1), Color.Lavender);
            //spriteBatch.Draw(Texture, _map.GetGroundCollisionBoxFromPoint(_bottomColliderRightmostPoint), new Rectangle(7, 4, 1, 1), Color.Lavender);
            //spriteBatch.Draw(Texture, _map.GetGroundCollisionBoxFromPoint(_bottomColliderLeftmostPoint), new Rectangle(7, 4, 1, 1), Color.Lavender);
        }

        private KeyboardState _previousKeyboardState;
        private List<Keys> _customPressedKeys = new();
        private bool _wasPlayerGroundedLastFrame; // Will be used for coyote timer
        public Vector2 Update(GameTime gameTime)
        {
            Vector2 movement = Vector2.Zero;
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.LeftShift) && _isPlayerGrounded && _coyoteTimerElapsedTime == 0)
            {
                _clingingOffset = 32;
                _isClinging = true;
                _speed = Global.Scale / 2;
            }
            else
            {
                _clingingOffset = 0;
                _isClinging = false;
                _speed = Global.Scale;
            }


            // Area that player will be the next frame
            Rectangle trejectory = new(_hitbox.X, _hitbox.Y, _hitbox.Width, _hitbox.Height + (int)_yAxisVelocity);

            bool isLeftColliderOnSolidBlock =
                _map.GetTileValueFromCoordinate(_bottomColliderLeftmostPoint) > 0 &&
                _map.GetGroundCollisionBoxFromPoint(_bottomColliderLeftmostPoint).Intersects(trejectory);

            bool isRightColliderOnSolidBlock =
                _map.GetTileValueFromCoordinate(_bottomColliderRightmostPoint) > 0 &&
                _map.GetGroundCollisionBoxFromPoint(_bottomColliderRightmostPoint).Intersects(trejectory);

            // Grounded check
            if (_initialDescent)
            {
                _isPlayerGrounded = false;
                if (_map.CurrentLevel == 0) _initialDescent = false;
            }
            else if ((isLeftColliderOnSolidBlock || isRightColliderOnSolidBlock) && !keyboardState.IsKeyDown(Keys.S)) // S is for going down
            {
                _isPlayerGrounded = true;

                // Clip to the ground level
                int yLevelToBeClipped = _map.GetGroundCollisionBoxFromPoint(_bottomColliderRightmostPoint).Y;
                if (yLevelToBeClipped == 0)
                    yLevelToBeClipped = _map.GetGroundCollisionBoxFromPoint(_bottomColliderLeftmostPoint).Y;

                Position.Y = yLevelToBeClipped - _hitbox.Height - 14; // Figure out what that 14 is ==> bc the block height is 3 pixels

                _yAxisVelocity = 0; // It's not a bug, it's a feature
            }
            else _isPlayerGrounded = false;

            // Coyote time 
            if (!_isPlayerGrounded && _wasPlayerGroundedLastFrame)
            {
                _coyoteTimerElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_coyoteTimerElapsedTime < COYOTE_TIME)
                    _isPlayerGrounded = _wasPlayerGroundedLastFrame;
            }
            else _coyoteTimerElapsedTime = 0;

            // Free fall check, can jump otherwise
            if (!_isPlayerGrounded)
            {
                int changeLevel = _map.CheckToChangeLevel(trejectory, _tutorial);

                switch (changeLevel)
                {
                    case 1:
                        //Debug.WriteLine("Increase Level");
                        Position.Y = Global.ResY - _hitbox.Height;
                        break;

                    case -1:
                        //Debug.WriteLine("Devrease Level");
                        Position.Y = -_hitbox.Height;
                        break;
                }

                if (_yAxisVelocity < TERMINAL_VELOCITY)
                    _yAxisVelocity += G_FORCE;
            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.W))
                {
                    _yAxisVelocity -= JUMPING_POWER;
                    _isPlayerGrounded = false;
                }
            }

            movement.Y += _yAxisVelocity;

            // Keep a list of pressed keys to detect which direction key was pressed last to ensure smooth movement
            Keys[] pressedKeys = keyboardState.GetPressedKeys();
            foreach (var key in pressedKeys)
            {
                if (!_customPressedKeys.Contains(key))
                    _customPressedKeys.Add(key);
            }

            // Remove released keys
            for (int i = _customPressedKeys.Count - 1; i >= 0; i--)
            {
                if (!pressedKeys.Contains(_customPressedKeys[i]))
                    _customPressedKeys.RemoveAt(i);
            }

            // Horizontal movement
            if (keyboardState.IsKeyDown(Keys.A))
            {
                if (_customPressedKeys.Contains(Keys.D))
                {
                    if (_customPressedKeys.IndexOf(Keys.A) > _customPressedKeys.IndexOf(Keys.D))
                        movement.X -= _speed;
                }
                else movement.X -= _speed;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                if (_customPressedKeys.Contains(Keys.A))
                {
                    if (_customPressedKeys.IndexOf(Keys.D) > _customPressedKeys.IndexOf(Keys.A))
                        movement.X += _speed;
                }
                else movement.X += _speed;
            }

            // Check screen borders and apply movement
            if (_hitbox.Right + movement.X > Global.ResX)
            {
                movement.X = Global.ResX - _hitbox.Right;
            }
            else if (_hitbox.Left + movement.X < 0)
            {
                movement.X = -_hitbox.Left;
            }

            if (_isClinging && _isPlayerGrounded && _coyoteTimerElapsedTime == 0)
            {
                // This works fine but... I thought I needed some additional check below:
                // (isLeftColliderOnSolidBlock ^ isRightColliderOnSolidBlock), for example
                // but it works perfectly without it and now I have no idea how this works
                if (movement.X > 0)
                {
                    if (_map.GetTileValueFromCoordinate(_bottomColliderRightmostPoint) == 0)
                        movement.X = 0;
                }
                else if (movement.X < 0)
                {
                    _bottomColliderLeftmostPoint.X += (int)movement.X;
                    if (_map.GetTileValueFromCoordinate(_bottomColliderLeftmostPoint) == 0)
                        movement.X = 0;
                }
            }

            if (movement.X > 0) _isFacingRight = true; else if (movement.X < 0) _isFacingRight = false;
            Position += movement;

            HandleAnimationState(gameTime, movement);

            // State updates
            _previousKeyboardState = keyboardState;
            _wasPlayerGroundedLastFrame = _isPlayerGrounded;

            _hitbox = CalculatePlayerHitbox();
            _bottomColliderLeftmostPoint = new Point(_hitbox.Left, _hitbox.Bottom);
            _bottomColliderRightmostPoint = new Point(_hitbox.Right, _hitbox.Bottom);

            // Return direction of movement for parallax 
            return movement;
        }

        private Rectangle CalculatePlayerHitbox()
        {

            Rectangle Hitbox = new(
                (int)(Position.X + TEXTURE_OFFSET_X),
                (int)(Position.Y + TEXTURE_OFFSET_Y),
                HITBOX_WIDTH,
                HITBOX_HEIGHT
            );
            return Hitbox;
        }

        private void HandleAnimationState(GameTime gameTime, Vector2 movement)
        {

            // Animation handler
            if (movement != Vector2.Zero)
            {
                if (movement.X != 0)
                {
                    if (movement.X > 0) _sourceRectangle.Y = TEXTURE_WIDTH + _clingingOffset;
                    else _sourceRectangle.Y = 0 + _clingingOffset;
                }

                if (_animationElapsedTime >= ANIMATION_TIME || _animationElapsedTime == 0)
                {
                    _animationElapsedTime = 0;
                    _sourceRectangle.X += TEXTURE_WIDTH;
                    if (_sourceRectangle.X > TEXTURE_WIDTH * ANIMATION_FRAMES_COUNT)
                        _sourceRectangle.X = TEXTURE_WIDTH;
                }

                _animationElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                _sourceRectangle.X = 0;
                _animationElapsedTime = 0;
                int offset = 0;
                if (_isFacingRight) offset = 16;

                _sourceRectangle.Y = _clingingOffset + offset;
            }
        }
    }
}
