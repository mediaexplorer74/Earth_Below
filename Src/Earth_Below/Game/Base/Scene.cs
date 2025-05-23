﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameManager.Base;

public interface IScene
{
    public void Load();
    public void Update(GameTime gameTime);
    public void Draw(SpriteBatch spriteBatch);
}