using System;

namespace Ohoy_Text_Version
{
    /// <summary>
    /// Class for the sprites in the game, specifically for islands and the ships.
    /// </summary>
    class Sprite
    {
        public char[,] CharacterMap;
        public int Width;
        public int Height;
        public ConsoleColor Color = ConsoleColor.White;

        public Sprite(int width, int height)
        {
            Width = width;
            Height = height;
            CharacterMap = new char[Width, Height];
        }
    }

    partial class Program
    {
        static Random random = new();
    }
}
