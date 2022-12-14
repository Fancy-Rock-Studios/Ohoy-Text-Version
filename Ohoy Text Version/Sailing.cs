using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Ohoy_Text_Version
{
    internal partial class Program
    {
        //Type Definitions
        enum CardinalDirection
        {
            North,
            South,
            West,
            East,
        }

        /// <summary>
        /// Class for the islands.
        /// </summary>
        class Island
        {
            public string Name;
            public Landmark Landmark;
            public bool Explored = false;
            public Point Position;
            public Sprite Sprite;
        }
        /// <summary>
        /// Class for the landmark which will be generated unto the islands.
        /// </summary>
        class Landmark
        {
            public string Name;
            public ConsoleColor Color;
            public string LandmarkSymbol;
        }
        /// <summary>
        /// Camera class, responsible for displaying the part of the map that the player sees.
        /// </summary>
        class Camera
        {
            public int Width;
            public int Height;
            public Point Position;
        }
        /// <summary>
        /// Ship class, 
        /// </summary>
        class Ship
        {
            public Point Position;
            public CardinalDirection CardinalDirection;
            public Dictionary<CardinalDirection, Sprite> Sprites = new Dictionary<CardinalDirection, Sprite>();
            public Point ShipCenter = new Point(4, 2);
        }
        class Map
        {
            public int Width;
            public int Height;
            public bool[,] FogOfWar;
            public Island TreasureIsland;

            public Map(int width, int heigth)
            {
                Width = width;
                Height = heigth;
                FogOfWar = new bool[Width, Height];
            }
        }
        struct ScreenCharacter
        {
            public char Symbol;
            public ConsoleColor ForegroundColor;
            public ConsoleColor BackgroundColor;
        }
        class Screen
        {
            public static ConsoleColor ForegroundColor;
            public static ConsoleColor BackgroundColor;
            public int Height;
            public int Width;
            public ScreenCharacter[,] Characters;

            public Screen(int width, int heigth)
            {
                Width = width;
                Height = heigth;
                Characters = new ScreenCharacter[Width, Height];
            }
            public void Clear()
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Characters[x, y].BackgroundColor = Screen.BackgroundColor;
                        Characters[x, y].ForegroundColor = Screen.ForegroundColor;
                        Characters[x, y].Symbol = ' ';
                    }
                }
            }
        }
        class Clue
        {
            public string Text;
        }
        class Journal
        {
            public List<Clue> Clues = new List<Clue>();
        }

        //Game Variables
        static int PrintPauseMilliseconds = 200;

        static string[] IslandNames;

        static List<Island> Islands;

        static List<Island> IslandClueList;

        static Ship PlayerShip;

        static List<Landmark> Landmarks = new List<Landmark>();

        static List<Landmark> LandmarksClueList;

        static List<Sprite> IslandSprites = new List<Sprite>();

        static Map AsciiSeaMap;

        static Camera PlayerCamera;

        static Screen CurrentScreen;

        static Screen NextScreen;

        static Journal PlayerJournal;

        static string[] cardinalClueSnippets = File.ReadAllLines("Text/CardinalCLueSnippets.txt");

        static bool ShouldQuit;

        static bool TreasureIslandDeclared = false;

        static bool OnTreasureIsland;

        public static bool IsOSPlatform(System.Runtime.InteropServices.OSPlatform osPlatform)
        {
            if (osPlatform == System.Runtime.InteropServices.OSPlatform.OSX)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        static void LoadData()
        {
            //Initialize Camera
            PlayerCamera = new Camera
            {
                Width = 149,
                Height = 50,
                Position = new Point(0, 0),
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.Clear();
                Console.CursorVisible = false;
                Console.WriteLine("Please maximize WindowSize before proceeding.");
                Console.WriteLine("Press any key to start the game!");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                //Initialize Console
                Console.Clear();
                Console.SetWindowSize(PlayerCamera.Width + 1, PlayerCamera.Height);
                Console.SetBufferSize(PlayerCamera.Width + 1, PlayerCamera.Height);
                Console.CursorVisible = false;
            }

            //Load IslandNames
            IslandNames = File.ReadAllLines("Text/IslandNames.txt");

            //Load IslandSprites
            for (int i = 1; i <= 9; i++)
            {
                IslandSprites.Add(ReadSpriteSailing($"Sprites/Islands/Shape{i}.txt"));
            }

            //Load Landmarks
            string[] filePaths = Directory.GetFiles("Sprites/Landmarks");
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string landmarkName = fileName.ToLowerInvariant();
                string[] landmarkInfo = File.ReadAllLines(filePath);
                ConsoleColor color = Enum.Parse<ConsoleColor>(landmarkInfo[0]);

                Landmark landmark = new Landmark()
                {
                    Name = landmarkName,
                    Color = color,
                    LandmarkSymbol = landmarkInfo[1],
                };
                Landmarks.Add(landmark);
            }
        }
        /// <summary>
        /// Method to which outputs a shape for our islands.
        /// </summary>
        /// <returns></returns>
        static int GenerateIslandShape()
        {
            int shapeNumber = random.Next(9);
            return shapeNumber;
        }
        /// <summary>
        /// This method generates a position/point for the island and checks whether if the island will collide with another island, continuing to generate points until the island doesn't collide with anything.
        /// </summary>
        /// <returns></returns>
        static Point GenerateIslandPoint()
        {
            while (true)
            {
                Point islandPoint = new Point(random.Next(AsciiSeaMap.Width - 30), random.Next(AsciiSeaMap.Height - 30));
                if (!(islandPoint.X > PlayerShip.Position.X + PlayerShip.ShipCenter.X - 20 && islandPoint.X < PlayerShip.Position.X + PlayerShip.ShipCenter.X + 5 && islandPoint.Y > PlayerShip.Position.Y + PlayerShip.ShipCenter.Y - 20 && islandPoint.Y < PlayerShip.Position.Y + PlayerShip.ShipCenter.Y + 5))
                {
                    bool doesOverlap = DoesOverlapIsland(islandPoint, 30, out _);
                    if (!doesOverlap)
                    {
                        return islandPoint;
                    }
                }

            }
        }

        static bool DeclareTreasureIsland(Island firstIsland)
        {
            while (true)
            {
                int islandIndex = random.Next(Islands.Count);
                if (Islands[islandIndex] == firstIsland)
                {
                    continue;
                }
                else
                {
                    AsciiSeaMap.TreasureIsland = Islands[islandIndex];
                    LandmarksClueList.Remove(AsciiSeaMap.TreasureIsland.Landmark);
                    return true;
                }
            }


        }
        static void ShuffleIslands(List<string> islandNames)
        {
            for (int i = 0; i <= islandNames.Count - 2; i++)
            {
                int j = random.Next(i, islandNames.Count);
                string temp = islandNames[i];
                islandNames[i] = islandNames[j];
                islandNames[j] = temp;
            }
        }
        static void InitializeObjects()
        {
            //Initialize The Map
            AsciiSeaMap = new Map(500, 250);

            TreasureIslandDeclared = false;

            //Initialize the FogOfWar
            AsciiSeaMap.FogOfWar = new Map(1000, 500).FogOfWar;
            for (int fogY = 0; fogY < AsciiSeaMap.Height; fogY++)
            {
                for (int fogX = 0; fogX < AsciiSeaMap.Width; fogX++)
                {
                    AsciiSeaMap.FogOfWar[fogX, fogY] = true;
                }
            }

            //Initialize Screens 
            CurrentScreen = new Screen(PlayerCamera.Width, PlayerCamera.Height);
            NextScreen = new Screen(PlayerCamera.Width, PlayerCamera.Height);

            //Initializing names for islands
            List<string> islandNames = new List<string>(IslandNames);

            //Copy the landmarks for clue purposes
            LandmarksClueList = new List<Landmark>(Landmarks);

            //Initializing landmarks for islands
            List<Landmark> landmarks = new List<Landmark>();
            for (int i = 0; i < 4; i++)
            {
                landmarks.AddRange(Landmarks);
            }

            //Initialize Ship
            PlayerShip = new Ship
            {
                Position = new Point(AsciiSeaMap.Width / 2, AsciiSeaMap.Height / 2),
                CardinalDirection = CardinalDirection.North,
            };

            PlayerShip.Sprites[CardinalDirection.North] = ReadSpriteSailing("Sprites/Ship/North.txt");
            PlayerShip.Sprites[CardinalDirection.South] = ReadSpriteSailing("Sprites/Ship/South.txt");
            PlayerShip.Sprites[CardinalDirection.East] = ReadSpriteSailing("Sprites/Ship/East.txt");
            PlayerShip.Sprites[CardinalDirection.West] = ReadSpriteSailing("Sprites/Ship/West.txt");

            //Clear a lil bit of fog
            ClearFogOfWar();

            //Generate all Islands
            Islands = new List<Island>();

            //Shuffle order of islands to randomize them
            ShuffleIslands(islandNames);

            //Make an island, 24 times.
            for (int i = 0; i < 24; i++)
            {
                Island newIsland = new Island
                {
                    Name = islandNames[i],
                    Landmark = landmarks[i],
                    Explored = false,
                    Position = GenerateIslandPoint(),
                    Sprite = IslandSprites[GenerateIslandShape()],
                };
                Islands.Add(newIsland);
            }

            //Add IslandLists
            IslandClueList = new List<Island>(Islands);

            //Initialize Journal
            PlayerJournal = new Journal();


        }
        /// <summary>
        /// This method prints the text within the confines of the console screen.
        /// </summary>
        /// <param name="text"></param>
        static void Print(string text)
        {
            //Split text into lines that don't exceed the window width.
            int maximumLineLength = Console.WindowWidth - 1;
            MatchCollection lineMatches = Regex.Matches(text, @"(.{1," + maximumLineLength + @"})(?:\s|$)");


            //Output each line.
            foreach (Match match in lineMatches)
            {
                Console.WriteLine(match.Groups[0].Value);
                Thread.Sleep(PrintPauseMilliseconds);
            }
        }
        //Method that writes on the screen wit Map-coordinates
        static void ScreenWriteMap(int mapX, int mapY, string text)
        {
            int screenX = mapX - PlayerCamera.Position.X;
            int screenY = mapY - PlayerCamera.Position.Y;
            ScreenWrite(screenX, screenY, text);
        }
        static void ScreenWriteMap(int mapX, int mapY, char symbol)
        {
            int screenX = mapX - PlayerCamera.Position.X;
            int screenY = mapY - PlayerCamera.Position.Y;
            ScreenWrite(screenX, screenY, symbol);
        }
        static void ScreenWrite(int x, int y, string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                ScreenWrite(x + i, y, text[i]);
            }
        }
        static void ScreenWrite(int x, int y, char symbol)
        {
            if (x >= 0 && x < NextScreen.Width && y >= 0 && y < NextScreen.Height)
            {
                NextScreen.Characters[x, y].Symbol = symbol;
                NextScreen.Characters[x, y].BackgroundColor = Screen.BackgroundColor;
                NextScreen.Characters[x, y].ForegroundColor = Screen.ForegroundColor;
            }

        }
        static void CenterCamera()
        {
            PlayerCamera.Position.X = PlayerShip.Position.X - (PlayerCamera.Width / 2);
            PlayerCamera.Position.Y = PlayerShip.Position.Y - (PlayerCamera.Height / 2);
            if (PlayerCamera.Position.X < 0)
            {
                PlayerCamera.Position.X = 0;
            }
            else if (PlayerCamera.Position.X > AsciiSeaMap.Width - PlayerCamera.Width)
            {
                PlayerCamera.Position.X = AsciiSeaMap.Width - PlayerCamera.Width;
            }
            if (PlayerCamera.Position.Y < 0)
            {
                PlayerCamera.Position.Y = 0;
            }
            else if (PlayerCamera.Position.Y > AsciiSeaMap.Height - PlayerCamera.Height)
            {
                PlayerCamera.Position.Y = AsciiSeaMap.Height - PlayerCamera.Height;
            }
        }
        /// <summary>
        /// A method which reads the sprites out of the textfiles, allowing the program to draw the sprites later.
        /// </summary>
        /// <param name="path">Which file to read.</param>
        /// <returns></returns>
        static Sprite ReadSpriteSailing(string path)
        {
            string[] spriteLines = File.ReadAllLines(path);

            ConsoleColor color;
            bool includesColorInformation = Enum.TryParse<ConsoleColor>(spriteLines[0], out color);

            //Determine Sprite Size Automatically
            int height = spriteLines.Length;
            int spriteStartingLineIndex = 0;
            if (includesColorInformation)
            {
                height--;
                spriteStartingLineIndex = 1;
            }

            int width = 0;

            for (int y = 0; y < height; y++)
            {
                string currentSpriteLine = spriteLines[y + spriteStartingLineIndex];
                int currentWidth = currentSpriteLine.Length;
                if (currentWidth > width)
                {
                    width = currentWidth;
                }
            }

            Sprite sprite = new Sprite(width, height);
            if (includesColorInformation)
            {
                sprite.Color = color;
            }

            //Set Up Sprite
            for (int y = 0; y < height; y++)
            {
                string currentSpriteLine = spriteLines[y + spriteStartingLineIndex];
                for (int x = 0; x < width; x++)
                {
                    if (x < currentSpriteLine.Length)
                    {
                        sprite.CharacterMap[x, y] = currentSpriteLine[x];
                    }
                    else
                    {
                        sprite.CharacterMap[x, y] = ' ';
                    }
                }
            }
            return sprite;
        }
        /// <summary>
        /// A method that draws the previously read sprite.
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="position"></param>
        static void DrawSpriteSailing(Sprite sprite, Point position)
        {
            for (int y = 0; y < sprite.Height; y++)
            {
                for (int x = 0; x < sprite.Width; x++)
                {
                    if (sprite.CharacterMap[x, y] == '.')
                    {
                        ScreenWriteMap(position.X + x, position.Y + y, ' ');
                    }
                    else if (sprite.CharacterMap[x, y] != ' ')
                    {
                        Screen.ForegroundColor = sprite.Color;
                        ScreenWriteMap(position.X + x, position.Y + y, sprite.CharacterMap[x, y]);
                    }
                }
            }
        }
        static void DrawShip()
        {
            Sprite currentShipSprite = PlayerShip.Sprites[PlayerShip.CardinalDirection];
            DrawSpriteSailing(currentShipSprite, PlayerShip.Position);
        }
        static void DrawIsland(Island island)
        {
            Sprite sprite = island.Sprite;
            Point position = island.Position;
            Landmark Landmark = island.Landmark;
            bool explored = island.Explored;

            Screen.ForegroundColor = sprite.Color;
            for (int y = 0; y < sprite.Height; y++)
            {
                for (int x = 0; x < sprite.Width; x++)
                {
                    int globalX = position.X + x;
                    int globalY = position.Y + y;
                    Point global = new Point(globalX, globalY);

                    if (explored == true && sprite.CharacterMap[x, y] == 'N')
                    {
                        DrawString(island.Name, global);
                    }
                    else if (sprite.CharacterMap[x, y] == '$')
                    {
                        Screen.ForegroundColor = Landmark.Color;
                        DrawString(Landmark.LandmarkSymbol, global);
                    }
                    else
                    {
                        if (explored == false && sprite.CharacterMap[x, y] == 'N')
                        {
                            ScreenWriteMap(globalX, globalY, ' ');
                        }
                        else if (sprite.CharacterMap[x, y] == '.')
                        {
                            ScreenWriteMap(globalX, globalY, ' ');
                        }
                        else if (sprite.CharacterMap[x, y] != ' ')
                        {
                            Screen.ForegroundColor = sprite.Color;
                            ScreenWriteMap(globalX, globalY, sprite.CharacterMap[x, y]);
                        }
                    }

                }
            }
        }
        static void DrawFogOfWar()
        {
            Screen.BackgroundColor = ConsoleColor.Black;
            for (int y = 0; y < PlayerCamera.Height; y++)
            {
                for (int x = 0; x < PlayerCamera.Width; x++)
                {
                    if (AsciiSeaMap.FogOfWar[x + PlayerCamera.Position.X, y + PlayerCamera.Position.Y])
                    {
                        ScreenWrite(x, y, ' ');
                    }
                }
            }
        }
        static void DrawString(string text, Point point)
        {
            ScreenWriteMap(point.X, point.Y, text);
        }
        static void DrawMap()
        {
            Screen.BackgroundColor = ConsoleColor.DarkBlue;
            NextScreen.Clear();
            foreach (Island island in Islands)
            {
                DrawIsland(island);
            }
            DrawShip();
            DrawFogOfWar();
        }
        //Method that screens.
        static void DrawNextScreen()
        {
            for (int y = 0; y < NextScreen.Height; y++)
            {
                for (int x = 0; x < NextScreen.Width; x++)
                {
                    bool backgroundIsDifferent = NextScreen.Characters[x, y].BackgroundColor != CurrentScreen.Characters[x, y].BackgroundColor;
                    bool foregroundIsDifferent = NextScreen.Characters[x, y].ForegroundColor != CurrentScreen.Characters[x, y].ForegroundColor;
                    bool symbolIsDifferent = NextScreen.Characters[x, y].Symbol != CurrentScreen.Characters[x, y].Symbol;
                    bool characterIsDifferent = symbolIsDifferent || foregroundIsDifferent || backgroundIsDifferent;
                    if (characterIsDifferent)
                    {
                        if (Console.ForegroundColor != NextScreen.Characters[x, y].ForegroundColor)
                        {
                            Console.ForegroundColor = NextScreen.Characters[x, y].ForegroundColor;
                        }
                        if (Console.BackgroundColor != NextScreen.Characters[x, y].BackgroundColor)
                        {
                            Console.BackgroundColor = NextScreen.Characters[x, y].BackgroundColor;
                        }
                        Console.SetCursorPosition(x, y);
                        Console.Write(NextScreen.Characters[x, y].Symbol);
                    }
                }
            }
            Screen temp = CurrentScreen;
            CurrentScreen = NextScreen;
            NextScreen = temp;
        }
        static bool DoesOverlapIsland(Point mapGlobalPositionCenter, int radius, out Island overlappingIsland)
        {
            // Go over all points
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                for (int offsetX = -radius; offsetX <= radius; offsetX++)
                {
                    Point currentPoint = new Point(mapGlobalPositionCenter.X + offsetX, mapGlobalPositionCenter.Y + offsetY);
                    bool doesOverlap = DoesOverlapIsland(currentPoint, out overlappingIsland);
                    if (doesOverlap)
                    {
                        return true;
                    }
                }
            }
            overlappingIsland = null;
            return false;
        }
        static bool DoesOverlapIsland(Point mapGlobalPosition, out Island overlappingIsland)
        {
            foreach (Island island in Islands)
            {
                // Calculate local coordinates relative to the island
                Point spriteCoordinates = new Point(mapGlobalPosition.X - island.Position.X, mapGlobalPosition.Y - island.Position.Y);

                //Check to see if we are out of bounds of the sprite.
                if (spriteCoordinates.Y < 0 || spriteCoordinates.Y >= island.Sprite.Height || spriteCoordinates.X < 0 || spriteCoordinates.X >= island.Sprite.Width)
                {
                    continue;
                }

                //If there is character at spriteCoordinates, return true.
                char spriteCharacter = island.Sprite.CharacterMap[spriteCoordinates.X, spriteCoordinates.Y];
                if (spriteCharacter != ' ' && spriteCharacter != 'N')
                {
                    overlappingIsland = island;
                    return true;
                }
            }

            overlappingIsland = null;
            return false;
        }
        static void ClearFogOfWar()
        {
            int radius = 11;
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                for (int offsetX = -radius * 2; offsetX <= radius * 2; offsetX++)
                {
                    Point currentPoint = new Point(PlayerShip.Position.X + PlayerShip.ShipCenter.X + offsetX, PlayerShip.Position.Y + PlayerShip.ShipCenter.Y + offsetY);

                    double deltaX = offsetX / 2;
                    double deltaY = offsetY;
                    double distance = Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2);
                    double square = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

                    if (currentPoint.Y >= 0 && currentPoint.Y < AsciiSeaMap.Height && currentPoint.X >= 0 && currentPoint.X < AsciiSeaMap.Width && square < radius)
                    {
                        AsciiSeaMap.FogOfWar[currentPoint.X, currentPoint.Y] = false;
                    }
                }
            }
        }
        #region introscreens
        /// <summary>
        /// This Method Presents the Title Screen, presenting the player with the game that they will be playing.
        /// </summary>
        static void PresentTitleScreen()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            string title = "OHOY! - A Text-Based Prototype";
            Console.SetCursorPosition((Console.WindowWidth - title.Length) / 2, Console.WindowHeight / 2);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Print(title);
            Console.SetCursorPosition((Console.WindowWidth - title.Length) / 2, (Console.WindowHeight / 2) + 3);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        /// <summary>
        /// This method introduces the player to the story and explains what the game will be about.
        /// </summary>
        static void PresentStoryScreen()
        {
            Console.Clear();
            string story = File.ReadAllText("Text/story.txt");
            Console.SetCursorPosition(0, Console.WindowHeight / 3);

            Print(story);

            Console.SetCursorPosition(Console.WindowWidth / 3, (Console.WindowHeight / 3) * 2);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        /// <summary>
        /// This method explain the few mechanics and keys needed to know.
        /// </summary>
        static void PresentTutorialScreen()
        {
            Console.Clear();
            string[] tutorial = File.ReadAllLines("Text/tutorial.txt");
            Console.SetCursorPosition(0, Console.WindowHeight / 3);
            Print(tutorial[0]);
            Console.SetCursorPosition(Console.WindowWidth / 3, Console.WindowHeight / 2);
            Console.WriteLine("Press any key to set sail!");
            Console.ReadKey();
        }
        #endregion

        static void PresentQuitScreen()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Clear();
            string[] quit = File.ReadAllLines("Text/Quit.txt");
            Console.SetCursorPosition(Console.WindowWidth / 3, Console.WindowHeight / 2);
            Print(quit[0]);
            Console.SetCursorPosition(Console.WindowWidth / 3, Console.WindowHeight / 2 + 1);
            Print(quit[1]);
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                ConsoleKey pressedKey = keyInfo.Key;
                if (pressedKey == ConsoleKey.Enter)
                {
                    return;
                }
                else if (pressedKey == ConsoleKey.Escape)
                {
                    ShouldQuit = true;
                    return;
                }
            }
        }
        static void PresentJournalScreen(bool typeOutLastClue)
        {

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Clear();
            while (true)
            {
                for (int i = 0; i < PlayerJournal.Clues.Count; i++)
                {
                    if (typeOutLastClue && i == PlayerJournal.Clues.Count - 1)
                    {
                        Clue lastClue = PlayerJournal.Clues[i];
                        Console.SetCursorPosition(15, 10 + i);
                        for (int j = 0; j < lastClue.Text.Length; j++)
                        {
                            Console.Write(lastClue.Text[j]);
                            Thread.Sleep(10);
                        }

                    }
                    else
                    {
                        Console.SetCursorPosition(15, 10 + i);
                        Print(PlayerJournal.Clues[i].Text);
                    }
                }
                Console.SetCursorPosition(Console.WindowWidth / 3, (Console.WindowHeight / 2) + 10);
                Console.WriteLine("Press ENTER to return to the ASCII-Sea!");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                ConsoleKey pressedKey = keyInfo.Key;
                if (pressedKey == ConsoleKey.Enter)
                {
                    CurrentScreen.Clear();
                    Console.Clear();
                    return;
                }
            }

        }

        static void PresentKeyScreen()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Clear();
            while (true)
            {
                string[] keyText = File.ReadAllLines("Text/KeyScreen.txt");
                for (int i = 0; i < keyText.Length; i++)
                {
                    Console.SetCursorPosition(Console.WindowWidth / 3, (Console.WindowHeight / 5) + i);
                    Print(keyText[i]);
                }
                Console.SetCursorPosition(Console.WindowWidth / 3, (Console.WindowHeight / 2) + 10);
                Console.WriteLine("Press ENTER to return to the ASCII-Sea!");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                ConsoleKey pressedKey = keyInfo.Key;
                if (pressedKey == ConsoleKey.Enter)
                {
                    CurrentScreen.Clear();
                    Console.Clear();
                    return;
                }
            }
        }

        /// <summary>
        /// This method houses the actual gameplay-loop, covering everything from sailing to fighting and clues.
        /// </summary>
        /// <returns>A boolean whether you have won at the treasure island.</returns>
        static bool DoGameplayLoop()
        {
            while (true)
            {

                DoSailingLoop();
                if (ShouldQuit)
                {
                    return false;
                }

                bool wonBattle = DoBattle(OnTreasureIsland);

                if (wonBattle)
                {
                    if (OnTreasureIsland)
                    {
                        return true;
                    }
                    else
                    {
                        ReceiveClue();
                        PresentJournalScreen(true);

                    }
                }
                else
                {
                    return false;
                }
            }
        }
        static void DoSailingLoop()
        {
            while (true)
            {

                //Draw Map
                CenterCamera();
                DrawMap();
                DrawNextScreen();

                //Handle input
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                ConsoleKey pressedKey = keyInfo.Key;


                //Figure out where we want to go

                Point mapShipCenter = new Point(PlayerShip.Position.X + PlayerShip.ShipCenter.X, PlayerShip.Position.Y + PlayerShip.ShipCenter.Y);
                Point pointIWantToGoTo = mapShipCenter;

                if (pressedKey == ConsoleKey.Escape)
                {
                    PresentQuitScreen();
                    if (ShouldQuit)
                    {
                        return;
                    }
                    CurrentScreen.Clear();
                    Console.Clear();
                    continue;
                }
                else if (pressedKey == ConsoleKey.J)
                {
                    PresentJournalScreen(false);
                    continue;
                }
                else if (pressedKey == ConsoleKey.K)
                {
                    PresentKeyScreen();
                    continue;
                }
                else if (pressedKey == ConsoleKey.P && DoesOverlapIsland(mapShipCenter, 4, out Island portingIsland))
                {
                    if (!TreasureIslandDeclared)
                    {
                        TreasureIslandDeclared = DeclareTreasureIsland(portingIsland);
                    }

                    OnTreasureIsland = portingIsland == AsciiSeaMap.TreasureIsland;

                    if (portingIsland.Explored != true)
                    {
                        portingIsland.Explored = true;
                        return;
                    }
                }

                else if (pressedKey == ConsoleKey.RightArrow && PlayerShip.Position.X < AsciiSeaMap.Width - PlayerShip.Sprites[CardinalDirection.East].Width)
                {
                    PlayerShip.CardinalDirection = CardinalDirection.East;
                    pointIWantToGoTo.X++;
                }
                else if (pressedKey == ConsoleKey.LeftArrow && PlayerShip.Position.X > 0)
                {
                    PlayerShip.CardinalDirection = CardinalDirection.West;
                    pointIWantToGoTo.X--;
                }
                else if (pressedKey == ConsoleKey.DownArrow && PlayerShip.Position.Y < AsciiSeaMap.Height - PlayerShip.Sprites[CardinalDirection.South].Height)
                {
                    PlayerShip.CardinalDirection = CardinalDirection.South;
                    pointIWantToGoTo.Y++;

                }
                else if (pressedKey == ConsoleKey.UpArrow && PlayerShip.Position.Y > 0)
                {
                    PlayerShip.CardinalDirection = CardinalDirection.North;
                    pointIWantToGoTo.Y--;
                }

                //Figure out if the point I want to go to is empty/valid.
                bool validSpace = !DoesOverlapIsland(pointIWantToGoTo, 1, out _);
                //Move the ship and camera if there is not an island at the position you are trying to go to.
                if (validSpace)
                {
                    PlayerShip.Position = new Point(pointIWantToGoTo.X - PlayerShip.ShipCenter.X, pointIWantToGoTo.Y - PlayerShip.ShipCenter.Y);
                    ClearFogOfWar();
                }
            }
        }
        static bool DoBattle(bool isBossBattle)
        {
            //TODO: Integrate Anton Prototype
            return MainCombat();
        }
        static void ReceiveClue()
        {
            int rand = random.Next(20);
            if (rand < 5 && LandmarksClueList.Count > 0)
            {
                LandmarkGeneration();
            }
            else if (rand >= 5 && IslandClueList.Count > 0)
            {
                CardinalClueGeneration();
            }
        }

        static void LandmarkGeneration()
        {
            //Determine what landmark
            int randomLandmark = random.Next(LandmarksClueList.Count);
            string landmark = LandmarksClueList[randomLandmark].Name;
            string landmarkSymbol = LandmarksClueList[randomLandmark].LandmarkSymbol;
            LandmarksClueList.Remove(LandmarksClueList[randomLandmark]);

            //Add clue
            Clue newClue = new Clue
            {
                Text = $"Apparently there are no {landmark} ({landmarkSymbol}) on the treasure island."
            };
            PlayerJournal.Clues.Add(newClue);
        }
        static void CardinalClueGeneration()
        {
            //Grab a random island to generate a clue for, then remove it from thelist of possible islands
            int randomIsland = random.Next(IslandClueList.Count);
            Island clueIsland = IslandClueList[randomIsland];
            IslandClueList.Remove(clueIsland);

            //Determine textsnippet for clue
            int randomSnippet = random.Next(cardinalClueSnippets.Length);
            string textSnippet = cardinalClueSnippets[randomSnippet];
            string direction = "North";
            //Figure out the difference between X.coordinates and Y
            int deltaX = clueIsland.Position.X - AsciiSeaMap.TreasureIsland.Position.X;
            int deltaY = clueIsland.Position.Y - AsciiSeaMap.TreasureIsland.Position.Y;
            int flipDeltaX = deltaX;
            int flipDeltaY = deltaY;
            if (deltaX < 0)
            {
                flipDeltaX = Math.Abs(deltaX);
            }
            else if (deltaY < 0)
            {
                flipDeltaY = Math.Abs(deltaY);
            }
            //Determine direction from treasure island 
            if (flipDeltaX > flipDeltaY)
            {
                if (deltaX > 0)
                {
                    //The clueisland is west of the treasure island.
                    direction = "East";
                }
                else
                {
                    //The clueisland is east of the treasure island.
                    direction = "West";
                }
            }
            else if (flipDeltaY > flipDeltaX)
            {
                if (deltaY > 0)
                {
                    //The Clueisland is north of the treasure island.
                    direction = "North";
                }
                else
                {
                    //The Clueisland is south of the treasure island.
                    direction = "South";
                }
            }
            Clue newClue = new Clue
            {
                Text = $"{textSnippet} {clueIsland.Name} lies {direction} of the treasure island."
            };
            PlayerJournal.Clues.Add(newClue);

        }
        /// <summary>
        /// Winscreen that displays if you manage to find the treasure!
        /// </summary>
        static void PresentWinScreen()
        {
            Console.Clear();
            CurrentScreen.Clear();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string winText = File.ReadAllText("Text/WinText.txt");
            Console.SetCursorPosition(0, Console.WindowHeight / 3);

            Print(winText);
        }
        /// <summary>
        /// Message that displays if you lost the game!
        /// </summary>
        static void PresentGameOverScreen()
        {
            Console.Clear();
            CurrentScreen.Clear();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string[] LoseText = File.ReadAllLines("Text/LoseText.txt");
            Console.SetCursorPosition(0, Console.WindowHeight / 3);
            Print(LoseText[0]);
            Console.SetCursorPosition(0, Console.WindowHeight / 3 + 2);
            Print(CurrentEnemy.Hint);
            Console.SetCursorPosition(Console.WindowWidth / 3, Console.WindowHeight / 3 + 4);
            Print(LoseText[1]);
        }
        static void Main(string[] args)
        {
            LoadData();

            //Main Game Loop
            while (true)
            {
                InitializeObjects();


                PresentTitleScreen();
                PresentStoryScreen();
                PresentTutorialScreen();
                Console.Clear();
                bool foundTreasure = DoGameplayLoop();
                if (ShouldQuit)
                {
                    return;
                }
                if (foundTreasure)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    PresentWinScreen();
                    Console.ReadKey();

                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    PresentGameOverScreen();
                    Console.ReadKey();
                }
            }
        }
    }
}
