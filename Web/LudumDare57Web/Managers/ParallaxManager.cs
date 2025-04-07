using LudumDare57Web.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace LudumDare57Web.Managers
{
    internal class ParallaxManager
    {
        private readonly int ResX = Global.ResX;
        private readonly int ResY = Global.ResY;
        private List<ParallaxLayer> _layers;
        private List<ParallaxLayer> _singleLayerParallaxes;

        private int _currentLevel = 3; // Starting level

        public int CurrentLevel
        {
            get { return _currentLevel; }
            set { _currentLevel = value; }
        }

        private bool _isMoving;
        private bool _isMovingForward;

        public bool IsMovingForward
        {
            set { _isMovingForward = value; }
        }

        public bool IsMoving
        {
            get { return _isMoving; }
            set { _isMoving = value; }
        }

        private const int TEXTURE_HEIGHT = 144;
        private const int TEXTURE_WIDTH = 256;
        private const int LAYER_COUNT = 4;
        private const float BASE_SPEED = .3f;
        //private const int ALWAYS_MOVING_LAYER_INDEX = 1; // Clouds for example
        public ParallaxManager(ContentManager Content)
        {
            Texture2D texture = Content.Load<Texture2D>("Textures/Background");
            _layers = new List<ParallaxLayer>();
            for (int i = 0; i < LAYER_COUNT; i++)
            {
                _layers.Add(new ParallaxLayer(texture, Vector2.Zero, ResX, ResY, new Rectangle(0, i * TEXTURE_HEIGHT, TEXTURE_WIDTH, TEXTURE_HEIGHT), i * -BASE_SPEED));
            }

            _singleLayerParallaxes = new List<ParallaxLayer>();
            for (int i = 0; i < LAYER_COUNT; i++)
            {
                _singleLayerParallaxes.Add(new ParallaxLayer(texture, Vector2.Zero, ResX, ResY, new Rectangle(TEXTURE_WIDTH, i * TEXTURE_HEIGHT, TEXTURE_WIDTH, TEXTURE_HEIGHT), i * -BASE_SPEED));
            }

        }

        // That is custom and messed up...
        private int GetLayerIndexFromLevel(int level)
        {
            switch (level)
            {
                case 0:
                    return 3;

                case 1:
                    return 2;

                case 2:
                    return 1;

                case 4:
                    return 0;

                default:
                    return 4;
            }
        }

        public void Update()
        {
            if (_isMoving)
            {
                if (_currentLevel == 3)
                {
                    int index = 0;
                    foreach (var layer in _layers)
                    {
                        //if (index == ALWAYS_MOVING_LAYER_INDEX && !_isMovingForward)
                        //layer.Update(true, BASE_SPEED * LAYER_COUNT); // Speed of the fastest moving layer

                        layer.Update(_isMovingForward);
                        index++;
                    }
                }
                else
                {
                    _singleLayerParallaxes[GetLayerIndexFromLevel(_currentLevel)].Update(_isMovingForward);
                }
            }
            //else _layers[ALWAYS_MOVING_LAYER_INDEX].Update(true);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_currentLevel == 3)
            {
                foreach (var layer in _layers)
                    layer.Draw(spriteBatch);
            }
            else _singleLayerParallaxes[GetLayerIndexFromLevel(_currentLevel)]?.Draw(spriteBatch);
        }
    }
}