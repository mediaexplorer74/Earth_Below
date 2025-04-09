using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameManager.Base
{
    internal class ParallaxLayer : Sprite
    {
        private readonly int ResX = Glob.ResX;
        private readonly int ResY = Glob.ResY;
        private float _speedFactor;

        private readonly Rectangle _srcRect;
        public ParallaxLayer(Texture2D Texture, Vector2 Position, int Width, int Height, Rectangle Source, float SpeedFactor) : base(Texture, Position, Width, Height)
        {
            _speedFactor = SpeedFactor;
            _srcRect = Source;
        }

        public void Update(bool isMovingForward, float speedAdjustment = 1f)
        {
            if (isMovingForward) Position.X += _speedFactor * speedAdjustment;
            else Position.X -= _speedFactor * speedAdjustment;

            if (Position.X <= -ResX || Position.X >= ResX)
            {
                Position.X = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, ResX, ResY), _srcRect, Color.White);
            spriteBatch.Draw(Texture, new Rectangle(Position.X < 0 ? (int)Position.X + ResX : (int)Position.X - ResX, (int)Position.Y, ResX, ResY), _srcRect, Color.White);
        }

    }
}