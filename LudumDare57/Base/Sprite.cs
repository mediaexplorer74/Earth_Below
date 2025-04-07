using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare57.Base
{
    internal class Sprite
    {
        public Texture2D Texture;
        public Vector2 Position;
        public int Width;
        public int Height;

        public Sprite(Texture2D texture, Vector2 position, int Width, int Height)
        {
            Texture = texture;
            Position = position;
            this.Width = Width;
            this.Height = Height;
        }

        public Rectangle Rect
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Rect, Color.White);
        }
    }
}