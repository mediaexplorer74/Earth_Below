using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;


namespace GameManager
{

    public static class Glob
    {
        public static int ResX { get; set; }
        public static int ResY { get; set; }
        public static int TextureSize { get; set; }
        public static int Scale { get; set; }
        public static int VirtualScreenWidth { get; set; }
        public static int VirtualScreenHeight { get; set; }
        public static int OffsetX { get; set; }
        public static int OffsetY { get; set; }
        public static int RenderTargetWidth { get; set; }
        public static int RenderTargetHeight { get; set; }
        public static float Time { get; set; }
        public static ContentManager Content { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static GraphicsDevice GraphicsDevice { get; set; }
        public static Point WindowSize { get; set; }

        public static void Update(GameTime gt)
        {
            double ts = gt.ElapsedGameTime.TotalSeconds;
            Time = (float)ts; 
        }            

    }
}
