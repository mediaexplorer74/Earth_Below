using LudumDare57Web.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare57Web.Entities
{
    internal class Projectile : Sprite
    {
        private Rectangle _sourceRectangle;
        private readonly int _speed = Global.Scale;

        // For dragon
        private Vector2 _direction;

        public Vector2 Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        private bool _toBeRemoved = false;
        public bool ToBeRemoved
        {
            get { return _toBeRemoved; }
            set { _toBeRemoved = value; }
        }
        public Projectile(Texture2D Texture, Vector2 Position, int Width, int Height, Rectangle sourceRectangle) : base(Texture, Position, Width, Height)
        {
            _sourceRectangle = sourceRectangle;
            _direction = Vector2.Zero;
        }

        public void Update()
        {
            // Remove if out of screen
            if (Rect.Top > Global.ResY || Rect.Right < 0 || Rect.Left > Global.ResX || Rect.Bottom < 0)
            {
                _toBeRemoved = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Rect, _sourceRectangle, Color.White);
        }
    }
}
