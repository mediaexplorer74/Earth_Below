using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LudumDare57Web.Interface
{
    internal class Tilemap
    {
        private readonly Texture2D _texture;

        // Settings 
        private const int _size = 16, _width = 16, _height = 9; // 16:9 aspect ration fully fills the screen, any other, there will be offset
        private readonly int _offsetX = (Global.ResX - _width * (_size * Global.Scale)) / 2;
        private readonly int _offsetY = (Global.ResY - _height * (_size * Global.Scale)) / 2;

        enum Tile
        {
            Empty,
            LeftEdgePiece,
            MiddlePiece,
            RightEdgePiece,
            LonePiece
        }

        // Map data
        private List<int[,]> _allMaps;
        private int _currentLevel;
        private int[,] _layout; // That's for current layout
        private const int _tileTypeCount = 4; // 0 indexed so n + 1
        private int _createdFileCount = 0;

        public int CurrentLevel
        {
            get { return _currentLevel; }
        }

        Tuple<int, int> _hoveringTile;
        public Tilemap(Texture2D texture)
        {
            _texture = texture;

            _layout = new int[_height, _width];

            
            _allMaps = new List<int[,]>();

            //string basePathString = "Maps/Level";
            //string fileExtension = ".dat";
            for (int iterator = 0; iterator < 5; iterator++)
            {
                int[,] layout = new int[_height, _width];
                for (int i = 0; i < _height; i++)
                {
                    for (int j = 0; j < _width; j++)
                    {
                        if (iterator == 0)
                        {
                            if (i == _height - 1)
                            {
                                if (j == 0) 
                                    layout[i, j] = (int)Tile.LeftEdgePiece;
                                else  if (j == _width - 1)
                                    layout[i, j] = (int)Tile.RightEdgePiece;
                                else
                                layout[i, j] = (int)Tile.MiddlePiece;
                            }
                        }
                        else if (i == 5 && j == 7)
                            layout[i, j] = (int)Tile.LeftEdgePiece;
                        else if (i == 5 && j == 8)
                            layout[i, j] = (int)Tile.RightEdgePiece;
                        else
                            layout[i, j] = (int) Tile.Empty;
                    }
                }

                //string currentFilePath = basePathString + i.ToString() + fileExtension;
                //layout = ReadTilemapFromDatFile(currentFilePath);
                _allMaps.Add(layout);
            }
            _currentLevel = 3;
            _layout = _allMaps[_currentLevel];

        }

        public int CheckToChangeLevel(Rectangle playerPosition, bool tutorial)
        {
            // 1 for increase level, -1 to decrease--0 means do nothing
            if (playerPosition.Bottom < 0)
            {
                if (_currentLevel == _allMaps.Count - 1)
                    return 0;

                if (!tutorial) ChangeLevel(true);
                return 1;
            }
            if (playerPosition.Top >= Global.ResY)
            {
                if (_currentLevel == 0)
                    return 0;

                if (!tutorial) ChangeLevel(false);
                return -1;
            }
            return 0;
        }

        private void ChangeLevel(bool increase)
        {
            if (increase)
            {
                _currentLevel++;
                _layout = _allMaps[_currentLevel];
            }
            else
            {
                _currentLevel--;
                _layout = _allMaps[_currentLevel];
            }
        }

        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;
        public void Update()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
            {
                _createdFileCount++;
                string path = "Maps/Map" + _createdFileCount.ToString() + ".dat";

                if (File.Exists(path))
                {
                    while (File.Exists(path))
                    {
                        _createdFileCount++;
                        path = "Maps/Map" + _createdFileCount.ToString() + ".dat";
                    }
                }
#if DEBUG
                WriteTilemapToDatFile(_layout, path);
#endif
            }

            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton != ButtonState.Pressed)
            {
                HandleBlockPlacing(mouseState.Position, true);
            }
            else HandleBlockPlacing(_previousMouseState.Position);
            _previousKeyboardState = keyboardState;
            _previousMouseState = mouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _layout.GetLength(1); i++)
            {
                for (int j = 0; j < _layout.GetLength(0); j++)
                {
                    if (_layout[j, i] == (int)Tile.Empty) continue;

                    Rectangle destinationRectangle = new(
                        _offsetX + i * (_size * Global.Scale),
                        _offsetY + j * (_size * Global.Scale),
                        _size * Global.Scale, _size * Global.Scale
                        );

                    Rectangle sourceRectangle = new(0 + _layout[j, i] * _size, 0, _size, _size);

                    spriteBatch.Draw(_texture, destinationRectangle, sourceRectangle, Color.White);

                }
            }

            if (_hoveringTile != null)
            {
                Rectangle hoveringTileDestination = new(
                            _offsetX + _hoveringTile.Item1 * (_size * Global.Scale),
                            _offsetY + _hoveringTile.Item2 * (_size * Global.Scale),
                            _size * Global.Scale, _size * Global.Scale
                            ); ;
                spriteBatch.Draw(_texture, hoveringTileDestination, new Rectangle(16, 16, 1, 1), new Color(Color.Black, .5f));

            }

        }

        public Tuple<int, int> GetPositionOnMapFromCoordinate(Point Coordinate)
        {
            int xPosition = (Coordinate.X - _offsetX) / (_size * Global.Scale);
            int yPosition = (Coordinate.Y - _offsetY) / (_size * Global.Scale);

            if (Coordinate.X < _offsetX || Coordinate.Y < _offsetY)
            {
                xPosition = -1;
                yPosition = -1;
            }

            return new Tuple<int, int>(xPosition, yPosition);
        }

        public int GetTileValueFromPosition(Tuple<int, int> Position)
        {
            if (Position.Item1 < 0 || Position.Item2 < 0 || Position.Item1 > _width - 1 || Position.Item2 > _height - 1)
                return -1;

            return _layout[Position.Item2, Position.Item1]; // Given position is X and Y but array is height first
        }

        public int GetTileValueFromCoordinate(Point Coordinate)
        {
            Tuple<int, int> Position = GetPositionOnMapFromCoordinate(Coordinate);
            return GetTileValueFromPosition(Position);
        }

        public static Point NormalizeCoordinate(Point Coordinate)
        {
            int adjustedX = -1;
            int adjustedY = -1;

            if (Coordinate.X < Global.OffsetX)
                Coordinate.X = -1;
            else
            {
                Coordinate.X -= Global.OffsetX;
                adjustedX = (Coordinate.X * Global.ResX / Global.RenderTargetWidth);
            }

            if (Coordinate.Y < Global.OffsetY)
                Coordinate.Y = -1;
            else
            {
                Coordinate.Y -= Global.OffsetY;
                adjustedY = (Coordinate.Y * Global.ResY / Global.RenderTargetHeight);
            }
            return new Point(adjustedX, adjustedY);
        }

        public Rectangle GetGroundCollisionBoxFromPoint(Point Location)
        {
            Tuple<int, int> PositionOnMap = GetPositionOnMapFromCoordinate(Location);
            if (GetTileValueFromPosition(PositionOnMap) == 0)
                return Rectangle.Empty;

            return new Rectangle(
                _offsetX + PositionOnMap.Item1 * (_size * Global.Scale),
                _offsetY + PositionOnMap.Item2 * (_size * Global.Scale) + Global.Scale * 12,
                _size * Global.Scale,
                _size * Global.Scale - Global.Scale * 12
                ); ;
        }

        public Rectangle GetGroundCollisionBoxFromLocation(Tuple<int, int> PositionOnMap)
        {
            return new Rectangle(
                _offsetX + PositionOnMap.Item1 * (_size * Global.Scale),
                _offsetY + PositionOnMap.Item2 * (_size * Global.Scale) + Global.Scale * 12,
                _size * Global.Scale,
                _size * Global.Scale - Global.Scale * 12
                ); ;
        }

        public static bool CheckIfColliderGrounded(Point LeftmostPoint, Point RightmostPoint)
        {
            Rectangle colliderRectangle = new Rectangle(LeftmostPoint.X, LeftmostPoint.Y, RightmostPoint.X - LeftmostPoint.X, Global.Scale);

            return false;
        }

#if DEBUG
        private static void WriteTilemapToDatFile(int[,] tilemap, string filePath)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                int rows = tilemap.GetLength(0);
                int cols = tilemap.GetLength(1);

                writer.Write(rows);
                writer.Write(cols);

                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        writer.Write(tilemap[row, col]);
                    }
                }
            }
            Debug.WriteLine("Saved the new map at [" + filePath + "].");
        }
#endif
        private static int[,] ReadTilemapFromDatFile(string filePath)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                int rows = reader.ReadInt32();
                int cols = reader.ReadInt32();

                int[,] tilemap = new int[rows, cols];

                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        tilemap[row, col] = reader.ReadInt32();
                    }
                }

                Debug.WriteLine("Loaded the map from [" + filePath + "] as current map.");
                return tilemap;
            }
        }

        public void HandleBlockPlacing(Point mouseCoordinate, bool clicked = false, bool normalize = true)
        {
            Point adjustedCursorPosition;
            if (normalize) adjustedCursorPosition = NormalizeCoordinate(mouseCoordinate);
            else adjustedCursorPosition = mouseCoordinate;

            Tuple<int, int> mousePosition = GetPositionOnMapFromCoordinate(adjustedCursorPosition);

            _hoveringTile = mousePosition;

            int hoveringTileValue = GetTileValueFromPosition(mousePosition);
            if (hoveringTileValue < 0) return;

            if (clicked)
            {
                // Empty tile is clicked, adding new block
                if (hoveringTileValue == 0)
                {
                    int consecutiveTileToTheLeftCount = 0, consecutiveTileToTheRightCount = 0;

                    // Check if there are blocks at either side of the block that will be placed
                    for (int i = _hoveringTile.Item1 + 1; i < _width; i++)
                    {
                        int value = _layout[_hoveringTile.Item2, i];

                        if (value > (int)Tile.Empty)
                            consecutiveTileToTheRightCount++;
                        else
                            break;
                    }

                    for (int i = _hoveringTile.Item1 - 1; i >= 0; i--)
                    {
                        int value = _layout[_hoveringTile.Item2, i];

                        if (value > (int)Tile.Empty)
                            consecutiveTileToTheLeftCount++;
                        else
                            break;
                    }

                    // No consecutive block
                    if (consecutiveTileToTheRightCount == 0 && consecutiveTileToTheLeftCount == 0)
                        _layout[_hoveringTile.Item2, _hoveringTile.Item1] = (int)Tile.LonePiece;
                    else if (consecutiveTileToTheLeftCount == 0) // No consecutive block to the left, but there are to the right
                    {
                        _layout[_hoveringTile.Item2, _hoveringTile.Item1] = (int)Tile.LeftEdgePiece;
                        if (consecutiveTileToTheRightCount > 1)
                            _layout[_hoveringTile.Item2, _hoveringTile.Item1 + 1] = (int)Tile.MiddlePiece; // 2 or more blocks
                        else _layout[_hoveringTile.Item2, _hoveringTile.Item1 + 1] = (int)Tile.RightEdgePiece; // Single block
                    }
                    else if (consecutiveTileToTheRightCount == 0) // No consecutive block to the right, but there are to the left
                    {
                        _layout[_hoveringTile.Item2, _hoveringTile.Item1] = (int)Tile.RightEdgePiece;
                        if (consecutiveTileToTheLeftCount > 1)
                            _layout[_hoveringTile.Item2, _hoveringTile.Item1 - 1] = (int)Tile.MiddlePiece; // 2 or more blocks
                        else _layout[_hoveringTile.Item2, _hoveringTile.Item1 - 1] = (int)Tile.LeftEdgePiece; // Single block
                    }
                    else // Blocks on either side
                    {
                        // Iterate the entire consecutive blocks to adjust tiles
                        for (int i = _hoveringTile.Item1 - consecutiveTileToTheLeftCount; i <= _hoveringTile.Item1 + consecutiveTileToTheRightCount; i++)
                        {
                            if (i == _hoveringTile.Item1 - consecutiveTileToTheLeftCount)
                                _layout[_hoveringTile.Item2, i] = (int)Tile.LeftEdgePiece; // Left edge
                            else if (i == _hoveringTile.Item1 + consecutiveTileToTheRightCount)
                                _layout[_hoveringTile.Item2, i] = (int)Tile.RightEdgePiece; // Rigth edge
                            else
                                _layout[_hoveringTile.Item2, i] = (int)Tile.MiddlePiece; // Eceryother block
                        }

                    }
                }
                else // Tile with existing block is clicked, removing the clicked block
                {
                    _layout[_hoveringTile.Item2, _hoveringTile.Item1] = (int)Tile.Empty; // Block to be removed
                    if (_hoveringTile.Item1 > 0) // If not map edge
                    {
                        if (_layout[_hoveringTile.Item2, _hoveringTile.Item1 - 1] == (int)Tile.MiddlePiece) // Block to the left is middle piece
                            _layout[_hoveringTile.Item2, _hoveringTile.Item1 - 1] = (int)Tile.RightEdgePiece; // So make it the new edge
                        else if (_layout[_hoveringTile.Item2, _hoveringTile.Item1 - 1] == (int)Tile.LeftEdgePiece) // Block to the left is an edge piece
                            _layout[_hoveringTile.Item2, _hoveringTile.Item1 - 1] = (int)Tile.LonePiece; // Now it is a single block
                    }
                    if (_hoveringTile.Item1 < _width - 1) // If not map edge
                    {
                        if (_layout[_hoveringTile.Item2, _hoveringTile.Item1 + 1] == (int)Tile.MiddlePiece) // Block to the right is a middle piece
                            _layout[_hoveringTile.Item2, _hoveringTile.Item1 + 1] = (int)Tile.LeftEdgePiece; // So make it the new edge
                        else if (_layout[_hoveringTile.Item2, _hoveringTile.Item1 + 1] == (int)Tile.RightEdgePiece) // block to the right is an edge piece
                            _layout[_hoveringTile.Item2, _hoveringTile.Item1 + 1] = (int)Tile.LonePiece; // Now it is a single block
                    }
                }

            }

        }


    }
}
