using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LudumDare57Web.Interface
{
    internal class TextRenderer
    {
        private Texture2D _texture;

        private readonly Color _textColor = new Color(233, 239, 236);

        private Queue<Rectangle> _drawQ;
        private Dictionary<char, Rectangle> _storedPositions;

        private readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>()
        {
            { "white", new Color(244, 254, 231) },
            { "red", new Color(210, 44, 88) },
            { "green", new Color(68, 174, 42) }

        };


        private int _fontWidth;
        private int _rowCount, _columnCount;
        private int _letterSpacing, _fontScale;
        private int _asciiStartAt, _asciiEndAt;
        public TextRenderer(Texture2D LettersSheet)
        {
            _texture = LettersSheet;
            _drawQ = new Queue<Rectangle>();
            _asciiStartAt = 32;
            _asciiEndAt = 126;
            _storedPositions = new Dictionary<char, Rectangle>(_asciiEndAt - _asciiStartAt + 1);
        }

        /// <summary>
        /// Sets the width of each character in the provided character sheet.
        /// Width and height of the provided character sheet must be divisible without remainder by the provided width.
        /// </summary>
        /// <param name="width"></param>
        /// <exception cref="Exception"></exception>
        public void SetFontWidthInPixels(int width)
        {
            _fontWidth = width;
            if (_texture.Width % width != 0 || _texture.Height % width != 0) throw new Exception("Provided character width " + width + ", is not compatible with provided character sheet " + _texture.Width + "x" + _texture.Height + ".");
            else
            {
                _rowCount = _texture.Width / width;
                _columnCount = _texture.Height / width;
                _storedPositions.Clear();
            }
        }

        /// <summary>
        /// Set range for allowed ascii characters, default is 32 to 126, from space to '~'.
        /// </summary>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <exception cref="Exception">Difference between each value determines the number of characters in the atlas.</exception>
        public void SetAsciiRange(int startAt, int endAt)
        {
            if (_asciiStartAt >= _asciiEndAt) throw new Exception("Range for allowed ascii characters cannot be negative or 0.");
            else
            {
                _asciiStartAt = startAt;
                _asciiEndAt = endAt;
                _storedPositions.Clear();
            }
        }

        /// <summary>
        /// Condense the space between each letter, higher the value, closer the characters. To scramble text and make unreadable, use 'CondenseLetterSpacingUnsafe()'.
        /// </summary>
        public void CondenseLetterSpacing(int width)
        {
            if (width <= _fontWidth / 2)
                _letterSpacing = width;
            else throw new Exception("Letter spacing cannot exceed half of the letter width, if this is intended, use SetLetterSpacingUnsafe()");
        }

        /// <summary>
        /// Condense the space between each letter, higher the value, closer the characters. Use it to scramble texts, make them possibly unreadable.
        /// </summary>
        /// <param name="width"></param>
        public void CondenseLetterSpacingUnsafe(int width)
        {
            _letterSpacing = width;
        }

        /// <summary>
        /// Expand the space between each letter, higher the value, further away the characters from eachother.
        /// </summary>
        /// <param name="width"></param>
        public void ExpandLetterSpacing(int width)
        {
            _letterSpacing = 0 - width;
        }

        /// <summary>
        /// Returns a rectangle with the width and height of a given text, when rendered with current settings.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Rectangle MeasureString(string text)
        {
            return new Rectangle(0, 0, (text.Length * ((_fontWidth * _fontScale) - (_letterSpacing * _fontScale))) + _letterSpacing * _fontScale, _fontWidth * _fontScale);
        }

        /// <summary>
        /// Scale the rendered text size, enlarge the size of a character n times.
        /// </summary>
        /// <param name="scale"></param>
        /// <exception cref="Exception">Font scale is the multiplication factor of the original size.</exception>
        public void SetFontScale(int scale)
        {
            if (scale > 0)
                _fontScale = scale;
            else throw new Exception("Font scale cannot be lower than 1 since it is used as a factor for the texture size.");
        }

        /// <summary>
        /// Main method of rendering a string to the screen.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        public void DrawString(SpriteBatch _spriteBatch, string text, Vector2 position, float transparency = 1, string color = "white")
        {
            ParseString(text);
            while (_drawQ.Count > 0)
            {
                _spriteBatch.Draw(_texture, new Rectangle((int)position.X, (int)position.Y, _fontWidth * _fontScale, _fontWidth * _fontScale), _drawQ.Peek(), _colors[color] * transparency);
                _drawQ.Dequeue();
                position.X += _fontWidth * _fontScale - _letterSpacing * _fontScale;
            }
        }

        /// <summary>
        /// When the text that is being rendered exceeds the given max width, wrap around to a new line and continue.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="maxWidth"></param>
        public void DrawStringWrapAround(SpriteBatch _spriteBatch, string text, Vector2 position, int maxWidth)
        {
            if (MeasureString(text).Width > maxWidth)
            {
                StoreString(" ");
                ParseString(text);
                int currentLineWidth = 0;
                int initialX = (int)position.X;
                int spacePerChar = _fontWidth * _fontScale - _letterSpacing * _fontScale;

                Queue<Rectangle> wordQ = new Queue<Rectangle>();

                while (_drawQ.Count > 0)
                {
                    while (_drawQ.Count > 0 && _drawQ.Peek() != _storedPositions[' '])
                    {
                        wordQ.Enqueue(_drawQ.Dequeue());
                    }
                    int spaceUsedForWord = spacePerChar * wordQ.Count;

                    try
                    {
                        if (spaceUsedForWord > maxWidth) throw new Exception("The string contains a word that alone exceeds the given max width. Use DrawStringWrapAroundHyphenate() instead.");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        return;
                    }

                    if (currentLineWidth + spaceUsedForWord <= maxWidth)
                    {
                        while (wordQ.Count > 0)
                        {
                            _spriteBatch.Draw(_texture, new Rectangle((int)position.X, (int)position.Y, _fontWidth * _fontScale, _fontWidth * _fontScale), wordQ.Dequeue(), _textColor);
                            position.X += spacePerChar;
                        }
                    }
                    else
                    {
                        position.Y += _fontWidth * _fontScale;
                        position.X = initialX;
                        currentLineWidth = 0;
                        while (wordQ.Count > 0)
                        {
                            _spriteBatch.Draw(_texture, new Rectangle((int)position.X, (int)position.Y, _fontWidth * _fontScale, _fontWidth * _fontScale), wordQ.Dequeue(), _textColor);
                            position.X += spacePerChar;
                        }
                    }
                    currentLineWidth += spaceUsedForWord;
                    if (_drawQ.Count > 0)
                    {
                        _spriteBatch.Draw(_texture, new Rectangle((int)position.X, (int)position.Y, _fontWidth * _fontScale, _fontWidth * _fontScale), _drawQ.Dequeue(), _textColor);
                        position.X += spacePerChar;
                        currentLineWidth += spacePerChar;
                    }

                }
            }
            else DrawString(_spriteBatch, text, position);

        }

        /// <summary>
        /// Similar to DrawStringWrapAround(), but also centers each line.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="maxWidth"></param>
        public void DrawStringWrapAroundCentered(SpriteBatch _spriteBatch, string text, Vector2 position, int maxWidth, Color color)
        {
            StoreString(" ");
            ParseString(text);
            int currentLineWidth = 0;
            int initialX = (int)position.X;
            int spacePerChar = _fontWidth * _fontScale - _letterSpacing * _fontScale;

            Queue<Rectangle> wordQ = new Queue<Rectangle>();
            Queue<Rectangle> lineQ = new Queue<Rectangle>();

            while (_drawQ.Count > 0)
            {
                int spaceUsedForWord;
                while (_drawQ.Count > 0 && _drawQ.Peek() != _storedPositions[' '])
                {
                    wordQ.Enqueue(_drawQ.Dequeue());
                }
                spaceUsedForWord = spacePerChar * wordQ.Count;

                try
                {
                    if (spaceUsedForWord > maxWidth) throw new Exception("The string contains a word that alone exceeds the given max width. Use DrawStringWrapAroundHyphenate() instead.");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return;
                }

                if (currentLineWidth + spaceUsedForWord <= maxWidth)
                {
                    while (wordQ.Count > 0)
                    {
                        lineQ.Enqueue(wordQ.Dequeue());
                    }
                    if (_drawQ.Count > 0 && _drawQ.Peek() == _storedPositions[' '])
                    {
                        lineQ.Enqueue(_drawQ.Dequeue());
                        currentLineWidth += spacePerChar;
                    }
                    currentLineWidth += spaceUsedForWord;
                }
                else
                {
                    position.X += (maxWidth - currentLineWidth + spacePerChar / 2) / 2;
                    while (lineQ.Count > 0)
                    {
                        _spriteBatch.Draw(_texture, new Rectangle((int)position.X, (int)position.Y, _fontWidth * _fontScale, _fontWidth * _fontScale), lineQ.Dequeue(), color);
                        position.X += spacePerChar;
                    }
                    lineQ.Clear();
                    currentLineWidth = 0;
                    position.X = initialX;
                    position.Y += _fontWidth * _fontScale;
                }
                if (_drawQ.Count == 0) // Exit condition met, last iteration, last line 
                {
                    if (wordQ.Count > 0)
                    {
                        currentLineWidth = spacePerChar * wordQ.Count;
                        position.X = initialX + (maxWidth - currentLineWidth) / 2;
                        while (wordQ.Count > 0)
                        {
                            _spriteBatch.Draw(_texture, new Rectangle((int)position.X, (int)position.Y, _fontWidth * _fontScale, _fontWidth * _fontScale), wordQ.Dequeue(), color);
                            position.X += spacePerChar;
                        }
                    }
                    else if (lineQ.Count > 0)
                    {
                        currentLineWidth = spacePerChar * lineQ.Count;
                        position.X = initialX + (maxWidth - currentLineWidth) / 2;
                        while (lineQ.Count > 0)
                        {
                            _spriteBatch.Draw(_texture, new Rectangle((int)position.X, (int)position.Y, _fontWidth * _fontScale, _fontWidth * _fontScale), lineQ.Dequeue(), color);
                            position.X += spacePerChar;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Similar to DrawStringWrapAround(), but the words will be hyphenated. Make sure you atlas has hyphen and space included.
        /// Per default behaviour:
        ///     When hyphenation occurs, a hyphen will be placed at the end of the line.
        ///     When a new line's first character is a space, it will be ommited.
        ///     When provided punctuations, these characters will not cause hyhenation, instead, they will be placed where hyphenation goes.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="maxWidth"></param>
        public void DrawStringWrapAroundHyphenate(SpriteBatch _spriteBatch, string text, Vector2 position, int maxWidth, string punctuations = "")
        {
            if (MeasureString(text).Width > maxWidth)
            {
                StoreString(" -");
                try
                {
                    if (!_storedPositions.TryGetValue('-', out Rectangle r) || !_storedPositions.TryGetValue(' ', out Rectangle r2))
                        throw new Exception("Cannot hyphenate since atlas does not contain hyphen and/or space.");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return;
                }

                List<Rectangle> punctuationRects = StoreString(punctuations);
                try
                {
                    if (punctuationRects.Count != punctuations.Length)
                        throw new Exception("Some of the puncuations provided do not exist in the atlas. Hyphenation might not work as intended.");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                ParseString(text);

                int currentLineWidth = 0;
                int initialX = (int)position.X;
                bool prevWasSpace = false;
                while (_drawQ.Count > 0)
                {
                    int spaceUsed = _fontWidth * _fontScale - _letterSpacing * _fontScale;
                    if (currentLineWidth == 0 && _drawQ.Peek() == _storedPositions[' '])
                    {
                        _drawQ.Dequeue();
                        continue;
                    }
                    currentLineWidth += spaceUsed;

                    if (currentLineWidth >= maxWidth - spaceUsed)
                    {
                        if (_drawQ.Peek() != _storedPositions[' '])
                        {
                            if (!prevWasSpace)
                            {
                                if (!punctuationRects.Contains(_drawQ.Peek()))
                                    _spriteBatch.Draw(_texture, new Rectangle((int)position.X, (int)position.Y, _fontWidth * _fontScale, _fontWidth * _fontScale), _storedPositions['-'], _textColor);
                                else
                                {
                                    _spriteBatch.Draw(_texture, new Rectangle((int)position.X, (int)position.Y, _fontWidth * _fontScale, _fontWidth * _fontScale), _drawQ.Peek(), _textColor);
                                    position.Y += _fontWidth * _fontScale;
                                    position.X = initialX;
                                    currentLineWidth = 0;
                                    _drawQ.Dequeue();
                                    continue;
                                }
                            }

                            position.Y += _fontWidth * _fontScale;
                            position.X = initialX;
                            currentLineWidth = spaceUsed;
                        }
                        else
                        {
                            position.Y += _fontWidth * _fontScale;
                            position.X = initialX;
                            currentLineWidth = 0;
                            _drawQ.Dequeue();
                            continue;
                        }
                    }
                    _spriteBatch.Draw(_texture, new Rectangle((int)position.X, (int)position.Y, _fontWidth * _fontScale, _fontWidth * _fontScale), _drawQ.Peek(), _textColor);
                    position.X += spaceUsed;
                    if (_drawQ.Peek() == _storedPositions[' '])
                        prevWasSpace = true;
                    else prevWasSpace = false;
                    _drawQ.Dequeue();
                }
            }
            else DrawString(_spriteBatch, text, position);
        }

        /// <summary>
        /// Find and--if not already--store the corresponding target rectangle of the characters in a string within atlas.
        /// </summary>
        /// <param name="text"></param>
        private void ParseString(string text)
        {
            foreach (char c in text)
            {
                if (_storedPositions.TryGetValue(c, out Rectangle rect)) _drawQ.Enqueue(rect);
                else
                {
                    int charPosition = (int)c - _asciiStartAt;

                    try
                    {
                        if (charPosition < 0 || charPosition > 95) throw new Exception("Only ascii from " + _asciiStartAt + (char)_asciiStartAt + " to " + _asciiEndAt + (char)_asciiEndAt + " allowed.");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        charPosition = (int)'?' - _asciiStartAt; /// Convert to question mark to handle 
                    }

                    Rectangle targetRectangle = GetTargetRectangle(charPosition);

                    _storedPositions.Add(c, targetRectangle);
                    _drawQ.Enqueue(targetRectangle);
                }
            }
        }

        /// <summary>
        /// Given a string, creates position stores for each character in the string.
        /// </summary>
        /// <param name="text"></param>
        private List<Rectangle> StoreString(string text)
        {
            List<Rectangle> storedChars = new List<Rectangle>();
            foreach (char c in text)
                if (!_storedPositions.TryGetValue(c, out Rectangle rect))
                {
                    int charPosition = (int)c - _asciiStartAt;

                    try
                    {
                        if (charPosition < 0 || charPosition > 95) throw new Exception("Only ascii from " + _asciiStartAt + (char)_asciiStartAt + " to " + _asciiEndAt + (char)_asciiEndAt + " allowed.");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        charPosition = (int)'?' - _asciiStartAt; /// Convert to question mark to handle 
                    }
                    Rectangle targetRectangle = GetTargetRectangle(charPosition);
                    _storedPositions.Add(c, targetRectangle);
                    storedChars.Add(targetRectangle);
                }
                else if (!storedChars.Contains(rect))
                    storedChars.Add(rect);
            return storedChars;
        }

        /// <summary>
        /// Given a character's position, get the rectangle corresponding to that character on the atlas.
        /// </summary>
        /// <param name="charPosition"></param>
        /// <returns></returns>
        private Rectangle GetTargetRectangle(int charPosition)
        {
            int charX = charPosition % _rowCount;
            int charY = charPosition / _rowCount;
            return new Rectangle(charX * _fontWidth, charY * _fontWidth, _fontWidth, _fontWidth);
        }
    }
}