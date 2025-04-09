using GameManager.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameManager.Interface;
using System;
using System.Collections.Generic;

namespace GameManager.Entities
{
    internal class Enemy : Sprite
    {
        private Rectangle _sourceRectangle;
        private const int TEXTURE_WIDTH = 16;

        private float _animationElapsedTime;
        private const float ANIMATION_TIME = 1.5f;
        private const int ANIMATION_FRAMES_COUNT = 2;

        private float _speed = Glob.Scale;

        private readonly Tilemap _map;

        private int _currentLevel = -1;

        private List<Projectile> _projectiles;
        private Rectangle _projectileSourceRenctangle;
        private bool _isAttacking;

        // For dragon
        private float _dragonAttackTimeElapsed;
        private const float DRAGON_ATTACK_TIME = .1f;

        // For Devil
        private int _devilAnimationIteration;
        private const int DEVIL_ANIMATION_ITERATION_LIMIT = 80;

        public Enemy(Texture2D Texture, Vector2 Position, int Width, int Height, Tilemap Map)
            : base(Texture, Position, Width, Height)
        {
            _map = Map;
            _sourceRectangle = new Rectangle(0, 0, TEXTURE_WIDTH, TEXTURE_WIDTH);
            _projectiles = new List<Projectile>();
        }

        public void Update(GameTime gameTime, Rectangle playerRectangle)
        {
            if (_currentLevel != _map.CurrentLevel)
                LevelIsChanged(_map.CurrentLevel, playerRectangle);

            float movement;

            List<Projectile> projectilesToBeRemoved = new();
            switch (_currentLevel)
            {
                // Devil
                case 0:
                    if (!_isAttacking)
                    {
                        if (_devilAnimationIteration % 4 == 0)
                        {
                            _sourceRectangle.X += TEXTURE_WIDTH;
                            if (_sourceRectangle.X > TEXTURE_WIDTH * ANIMATION_FRAMES_COUNT - 1)
                            {
                                _sourceRectangle.X = 0;
                            }
                        }

                        _devilAnimationIteration++;
                        if (_devilAnimationIteration >= DEVIL_ANIMATION_ITERATION_LIMIT)
                        {
                            _isAttacking = true;
                            _devilAnimationIteration = 0;
                        }
                    }
                    else
                    {
                        _devilAnimationIteration++;
                        if (_devilAnimationIteration >= DEVIL_ANIMATION_ITERATION_LIMIT)
                        {
                            _isAttacking = false;
                            _devilAnimationIteration = 0;
                        }
                        if (_projectiles.Count == 0)
                        {
                            Projectile projectileToBeAdded = new Projectile(Texture, Position, Width, Height, _projectileSourceRenctangle);
                            projectileToBeAdded.Direction = new Vector2(playerRectangle.Center.X, playerRectangle.Center.Y) - new Vector2(projectileToBeAdded.Rect.Center.X, projectileToBeAdded.Rect.Center.Y);
                            _projectiles.Add(projectileToBeAdded);
                        }
                    }

                    foreach (var projectile in _projectiles)
                    {
                        Vector2 move = projectile.Direction;
                        move.Normalize();
                        move *= _speed * 2;
                        projectile.Position += move;
                        projectile.Update();

                        int touchingTileValue = _map.GetTileValueFromCoordinate(projectile.Rect.Center);
                        if (touchingTileValue > 0)
                            _map.HandleBlockPlacing(projectile.Rect.Center, true, false);

                        if (projectile.ToBeRemoved)
                            projectilesToBeRemoved.Add(projectile);
                    }

                    foreach (var projectile in projectilesToBeRemoved)
                    {
                        _projectiles.Remove(projectile);
                    }
                    break;

                // Dragon
                case 1:
                    HandleAnimation(gameTime);
                    if (_isAttacking)
                    {
                        _dragonAttackTimeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (_dragonAttackTimeElapsed >= DRAGON_ATTACK_TIME)
                        {
                            Projectile projectileToBeAdded = new Projectile(Texture, Position, Width, Height, _projectileSourceRenctangle);
                            projectileToBeAdded.Direction = new Vector2(playerRectangle.Center.X, playerRectangle.Center.Y) - projectileToBeAdded.Position;
                            _projectiles.Add(projectileToBeAdded);
                            _dragonAttackTimeElapsed = 0;
                        }
                    }
                    else _dragonAttackTimeElapsed = 0;

                    foreach (var projectile in _projectiles)
                    {
                        Vector2 move = projectile.Direction;
                        move.Normalize();
                        move *= _speed;
                        projectile.Position += move;
                        projectile.Update();

                        int touchingTileValue = _map.GetTileValueFromCoordinate(projectile.Rect.Center);
                        if (touchingTileValue > 0)
                            _map.HandleBlockPlacing(projectile.Rect.Center, true, false);

                        if (projectile.ToBeRemoved)
                            projectilesToBeRemoved.Add(projectile);
                    }

                    foreach (var projectile in projectilesToBeRemoved)
                    {
                        _projectiles.Remove(projectile);
                    }

                    break;

                // Golbins
                case 2:
                    movement = Position.Y - playerRectangle.Y;
                    HandleGoblinAttack(gameTime);
                    if (Math.Abs(movement) > TEXTURE_WIDTH / 2)
                    {
                        if (movement > 0)
                        {
                            Position.Y -= _speed;
                        }
                        else Position.Y += _speed;
                    }

                    if (_projectiles.Count > 1)
                    {
                        if (_projectiles[0].Rect.Intersects(_projectiles[1].Rect))
                        {
                            _projectiles[0].ToBeRemoved = true;
                            _projectiles[1].ToBeRemoved = true;
                        }
                    }

                    int iteration = 0;
                    foreach (var projectile in _projectiles)
                    {
                        if (iteration % 2 == 0)
                            projectile.Position.X += _speed;
                        else projectile.Position.X -= _speed;

                        projectile.Update();

                        int touchingTileValue = _map.GetTileValueFromCoordinate(projectile.Rect.Center);
                        if (touchingTileValue > 0)
                            _map.HandleBlockPlacing(projectile.Rect.Center, true, false);

                        if (projectile.ToBeRemoved)
                            projectilesToBeRemoved.Add(projectile);

                        iteration++;
                    }

                    foreach (var projectile in projectilesToBeRemoved)
                    {
                        _projectiles.Remove(projectile);
                    }
                    break;

                case 3:
                    break;

                // Zeus
                case 4:

                    movement = Position.X - playerRectangle.X;
                    HandleAnimation(gameTime);
                    if (!_isAttacking && Math.Abs(movement) > TEXTURE_WIDTH / 2)
                    {
                        if (movement > 0)
                        {
                            Position.X -= _speed;
                        }
                        else Position.X += _speed;

                    }
                    else
                    {
                        if (_projectiles.Count <= 0)
                        {
                            _projectiles.Add(new Projectile(Texture, Position, Width, Height, _projectileSourceRenctangle));
                        }
                    }

                    foreach (var projectile in _projectiles)
                    {
                        projectile.Position.Y += _speed;
                        projectile.Update();

                        int touchingTileValue = _map.GetTileValueFromCoordinate(projectile.Rect.Center);
                        if (touchingTileValue > 0)
                            _map.HandleBlockPlacing(projectile.Rect.Center, true, false);

                        if (projectile.ToBeRemoved)
                            projectilesToBeRemoved.Add(projectile);
                    }

                    foreach (var projectile in projectilesToBeRemoved)
                    {
                        _projectiles.Remove(projectile);
                    }

                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_currentLevel)
            {
                // Devil
                case 0:
                    spriteBatch.Draw(Texture, Rect, _sourceRectangle, Color.White);
                    break;

                // Dragon
                case 1:
                    spriteBatch.Draw(Texture, Rect, _sourceRectangle, Color.White);
                    break;

                // Goblins
                case 2:
                    spriteBatch.Draw(Texture, Rect, new Rectangle(TEXTURE_WIDTH, _sourceRectangle.Y, TEXTURE_WIDTH, TEXTURE_WIDTH), Color.White);
                    spriteBatch.Draw(Texture, new Rectangle(Glob.ResX - Width, Rect.Y, Width, Height), _sourceRectangle, Color.White);
                    break;


                case 3:
                    break;

                // Zeus
                case 4:
                    spriteBatch.Draw(Texture, Rect, _sourceRectangle, Color.White);
                    break;
            }

            foreach (var projectile in _projectiles)
            {
                projectile.Draw(spriteBatch);
            }
        }

        private void HandleGoblinAttack(GameTime gameTime)
        {
            if (_animationElapsedTime >= ANIMATION_TIME || _animationElapsedTime == 0)
            {
                _isAttacking = !_isAttacking;
                _animationElapsedTime = 0;
                if (_isAttacking && _projectiles.Count == 0)
                {
                    _projectiles.Add(new Projectile(Texture, Position, Width, Height, _projectileSourceRenctangle));
                    _projectiles.Add(new Projectile(Texture, new Vector2(Glob.ResX - Width, Rect.Y), Width, Height, _projectileSourceRenctangle));
                }
            }
        }

        private void HandleAnimation(GameTime gameTime)
        {
            if (_animationElapsedTime >= ANIMATION_TIME || _animationElapsedTime == 0)
            {
                _isAttacking = true;
                _animationElapsedTime = 0;
                _sourceRectangle.X += TEXTURE_WIDTH;
                if (_sourceRectangle.X > TEXTURE_WIDTH * ANIMATION_FRAMES_COUNT - 1)
                {
                    _sourceRectangle.X = 0;
                    _isAttacking = false;
                }
            }

            _animationElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        private void LevelIsChanged(int changedTo, Rectangle playerRectangle)
        {
            _currentLevel = changedTo;
            _sourceRectangle = new Rectangle(0, changedTo * TEXTURE_WIDTH, TEXTURE_WIDTH, TEXTURE_WIDTH);
            _projectiles.Clear();
            _projectileSourceRenctangle = new Rectangle(TEXTURE_WIDTH * 2, TEXTURE_WIDTH * changedTo, TEXTURE_WIDTH, TEXTURE_WIDTH);
            _isAttacking = false;
            _animationElapsedTime = 0;

            switch (_currentLevel)
            {
                //Devil
                case 0:
                    _devilAnimationIteration = 0;
                    Position.X = Glob.ResX / 2 - Width / 2;
                    Position.Y = Glob.ResY / 2 - Height / 2;
                    break;

                // Dragon
                case 1:
                    _dragonAttackTimeElapsed = 0;
                    Position.X = Glob.ResX / 2 - Width / 2;
                    Position.Y = Glob.ResY / 2 - Height / 2;
                    break;

                // Goblins
                case 2:
                    Position.Y = playerRectangle.Y;
                    Position.X = 0;
                    break;

                case 3:
                    break;

                // Zeus
                case 4:
                    Position = new Vector2(playerRectangle.X, 0);
                    _projectiles = new List<Projectile>();
                    break;
            }
        }
    }
}
