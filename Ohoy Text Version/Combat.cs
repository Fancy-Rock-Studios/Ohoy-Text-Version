using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Ohoy_Text_Version
{
    //Player Class
    class Player
    {
        public Sprite Sprite;
        public int HP;
        public Point Position;
        public bool IsAttacking;
        public bool IsAttacked;
    }

    //Enemy Class
    class Enemy
    {
        public string Hint;
        public int HP;
        public int Damage;
        public Sprite IdleSprite;
        public List<Sprite> AttackSprites = new List<Sprite>();
        public Point Position;
        public TimeSpan AttackTime;
        public TimeSpan MovementTime;
        public bool IsAttacking = false;
        public bool IsAttacked;
        public int CurrentAttackIndex = 0;
        public string Name;
    }

    //Weapon Class
    class Weapon
    {
        public string Name;
        public string Description;
        public Sprite IdleSprite;
        public Sprite AttackSprite;
        public TimeSpan AttackTime;
        public int Damage;
        public int Ammo;
        public Point Position;
    }

    internal partial class Program
    {
        //Constants
        static readonly TimeSpan DamageCooldown = TimeSpan.FromSeconds(1);

        //Combat Variables
        static int ArenaWidth;
        static int ArenaHeight;
        static ConsoleColor ArenaBackgroundColor;

        static Player Player;

        static Weapon Cutlass;
        static Weapon Dagger;
        static Weapon BoatAxe;
        static Weapon Flintlock;
        static Weapon CurrentWeapon;

        static Enemy Skeleton;
        static Enemy Seaweed;
        static Enemy Bat;
        static Enemy Mimic;
        static Enemy StoneGolem;
        static Enemy GiantCrab;
        static Enemy CurrentEnemy;

        #region Initialize Objects

        static void InitializeArena()
        {
            ArenaWidth = 150;
            ArenaHeight = 50;
            ArenaBackgroundColor = ConsoleColor.DarkYellow;
        }

        static void InitializeEnemies()
        {
            GiantCrab = new Enemy();
            GiantCrab.IdleSprite = ReadSpriteCombat("Sprites/Enemies/GiantCrabIdle.txt");
            GiantCrab.AttackSprites.Add(ReadSpriteCombat("Sprites/Enemies/GiantCrabAttack1.txt"));
            GiantCrab.AttackSprites.Add(ReadSpriteCombat("Sprites/Enemies/GiantCrabAttack2.txt"));
            GiantCrab.AttackSprites.Add(ReadSpriteCombat("Sprites/Enemies/GiantCrabAttack3.txt"));
            GiantCrab.AttackTime = TimeSpan.FromSeconds(2);
            GiantCrab.MovementTime = TimeSpan.FromSeconds(0.5);
            GiantCrab.Name = "The Giant Crab";
            GiantCrab.HP = 1500;
            GiantCrab.Damage = 40;
            GiantCrab.Hint = "The Giant Crab is the most powerful and dangerous enemy in the ASCII-Sea. Weaken it with well aimed shots from your flintlock, then go in for the kill!";

            Seaweed = new Enemy();
            Seaweed.IdleSprite = ReadSpriteCombat("Sprites/Enemies/SeaweedIdle.txt");
            Seaweed.AttackSprites.Add(ReadSpriteCombat("Sprites/Enemies/SeaweedAttack.txt"));
            Seaweed.AttackTime = TimeSpan.FromSeconds(1);
            Seaweed.MovementTime = TimeSpan.FromSeconds(1.2);
            Seaweed.Name = "Slimy Seaweed";
            Seaweed.HP = 450;
            Seaweed.Damage = 20;
            Seaweed.Hint = "The Seaweed is the only stationary enemy in the ASCII-Sea. Avoid getting slapped by it, and you'll be fine.";

            Bat = new Enemy();
            Bat.IdleSprite = ReadSpriteCombat("Sprites/Enemies/Bat.txt");
            Bat.AttackSprites.Add(ReadSpriteCombat("Sprites/Enemies/Bat.txt"));
            Bat.AttackTime = TimeSpan.FromSeconds(1);
            Bat.MovementTime = TimeSpan.FromSeconds(0.2);
            Bat.Name = "Ferocious Bat";
            Bat.HP = 100;
            Bat.Damage = 10;
            Bat.Hint = "The Bat is the quickest, but weakest enemy in the ASCII-Sea. Try to fight fire with fire, by using a quick weapon, or use the slow but powerful Boat Axe for a quick kill!";

            Mimic = new Enemy();
            Mimic.IdleSprite = ReadSpriteCombat("Sprites/Enemies/MimicIdle.txt");
            Mimic.AttackSprites.Add(ReadSpriteCombat("Sprites/Enemies/MimicAttack.txt"));
            Mimic.AttackTime = TimeSpan.FromSeconds(1.3);
            Mimic.MovementTime = TimeSpan.FromSeconds(1.3);
            Mimic.Name = "Sneaky Mimic";
            Mimic.HP = 750;
            Mimic.Damage = 30;
            Mimic.Hint = "The Mimic is a slow, but devious opponent. The Boat Axe is a good choice against this fiend, but don't get caught off guard by it!";

            Skeleton = new Enemy();
            Skeleton.IdleSprite = ReadSpriteCombat("Sprites/Enemies/SkeletonIdle.txt");
            Skeleton.AttackSprites.Add(ReadSpriteCombat("Sprites/Enemies/SkeletonAttack.txt"));
            Skeleton.AttackTime = TimeSpan.FromSeconds(1);
            Skeleton.MovementTime = TimeSpan.FromSeconds(1);
            Skeleton.Name = "Undead Skeleton";
            Skeleton.HP = 250;
            Skeleton.Damage = 30;
            Skeleton.Hint = "The Skeleton uses a Cutlass to fight you. Keep this in mind, as you fight it. Know your tools, and slay it!";

            StoneGolem = new Enemy();
            StoneGolem.IdleSprite = ReadSpriteCombat("Sprites/Enemies/StoneGolemIdle.txt");
            StoneGolem.AttackSprites.Add(ReadSpriteCombat("Sprites/Enemies/StoneGolemAttack1.txt"));
            StoneGolem.AttackSprites.Add(ReadSpriteCombat("Sprites/Enemies/StoneGolemAttack2.txt"));
            StoneGolem.AttackSprites.Add(ReadSpriteCombat("Sprites/Enemies/StoneGolemAttack3.txt"));
            StoneGolem.AttackTime = TimeSpan.FromSeconds(3);
            StoneGolem.MovementTime = TimeSpan.FromSeconds(3);
            StoneGolem.Name = "Mighty Stone Golem";
            StoneGolem.HP = 1500;
            StoneGolem.Damage = 40;
            StoneGolem.Hint = "The Golem is a slow but powerful opponent. While he cannot move around, his attacks are still deadly. Stay cautious.";
        }

        static void InitializeWeapons()
        {
            BoatAxe = new Weapon();
            BoatAxe.IdleSprite = ReadSpriteCombat("Sprites/Weapons/AxeIdle.txt");
            BoatAxe.AttackSprite = ReadSpriteCombat("Sprites/Weapons/AxeAttack.txt");
            BoatAxe.AttackTime = TimeSpan.FromSeconds(2);
            BoatAxe.Ammo = 1;
            BoatAxe.Damage = 50;
            BoatAxe.Name = "Boat Axe";
            BoatAxe.Description = "This weapon has some heavy strength, but it's speed is sluggish, and will leave you vulnerable.";

            Dagger = new Weapon();
            Dagger.IdleSprite = ReadSpriteCombat("Sprites/Weapons/DaggerIdle.txt");
            Dagger.AttackSprite = ReadSpriteCombat("Sprites/Weapons/DaggerAttack.txt");
            Dagger.AttackTime = TimeSpan.FromSeconds(0.5);
            Dagger.Ammo = 1;
            Dagger.Damage = 10;
            Dagger.Name = "Rusty Dagger";
            Dagger.Description = "A dagger, covered in rust. It won't hurt much, but you'll be able to hit quickly.";

            Cutlass = new Weapon();
            Cutlass.IdleSprite = ReadSpriteCombat("Sprites/Weapons/CutlassIdle.txt");
            Cutlass.AttackSprite = ReadSpriteCombat("Sprites/Weapons/CutlassAttack.txt");
            Cutlass.AttackTime = TimeSpan.FromSeconds(1);
            Cutlass.Ammo = 1;
            Cutlass.Damage = 30;
            Cutlass.Name = "Ye Olde Cutlass";
            Cutlass.Description = "A classic weapon, and a trusty choice in defeating your opponents.";

            Flintlock = new Weapon();
            Flintlock.IdleSprite = ReadSpriteCombat("Sprites/Weapons/FlintlockIdle.txt");
            Flintlock.AttackSprite = ReadSpriteCombat("Sprites/Weapons/FlintlockAttack.txt");
            Flintlock.AttackTime = TimeSpan.FromSeconds(1);
            Flintlock.Ammo = 1;
            Flintlock.Damage = 75;
            Flintlock.Name = "Sinclair's Flintlock";
            Flintlock.Description = "A flintlock pistol, taken from the corpse of your rival, Sinclair. It only has one bullet.";
        }

        static void InitializePlayer()
        {
            Player = new Player();
            Player.Sprite = ReadSpriteCombat("Sprites/Player/Player.txt");
            Player.HP = 500;
        }

        static Sprite ReadSpriteCombat(string path)
        {
            string[] spriteLines = File.ReadAllLines(path);

            // Determine Sprite Color
            string colorText = spriteLines[0];
            ConsoleColor color = Enum.Parse<ConsoleColor>(colorText);

            // Determine Sprite Size
            string[] spriteSizeParts = spriteLines[1].Split('x');
            string spriteHeightText = spriteSizeParts[1];
            string spriteWidthText = spriteSizeParts[0];
            int width = Convert.ToInt32(spriteWidthText);
            int height = Convert.ToInt32(spriteHeightText);


            //Setting Up The Sprite
            Sprite sprite = new Sprite(width, height);
            sprite.Color = color;

            for (int y = 0; y < height; y++)
            {
                string currentSpriteLine = spriteLines[y + 3];
                for (int x = 0; x < width; x++)
                {
                    sprite.CharacterMap[x, y] = currentSpriteLine[x];
                }
            }
            return sprite;
        }

        #endregion

        #region Drawing

        static void DrawCombat()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write($"Player HP: {Player.HP}                                                                                                       {CurrentEnemy.Name} HP: {CurrentEnemy.HP}");
            Console.WriteLine();
            Console.WriteLine();
            Console.Write($"Weapon Equipped: {CurrentWeapon.Name}");
            if (CurrentWeapon == Flintlock)
            {
                Console.Write($"  --  Ammo Count:{CurrentWeapon.Ammo}");
            }
            Console.WriteLine();
            Console.WriteLine(CurrentWeapon.Description);
            Console.WriteLine("C Key: Cycle/Switch Weapons     V Key: Attack");
            Console.WriteLine("Arrow Keys: Move");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------------------------------------------");

            DrawPlayer();
            DrawEnemy();
        }

        //Draw Sprite
        static void DrawSpriteCombat(Sprite sprite, Point position, ConsoleColor? color = null)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }
            else
            {
                Console.ForegroundColor = sprite.Color;
            }
            for (int y = 0; y < sprite.Height; y++)
            {
                for (int x = 0; x < sprite.Width; x++)
                {
                    Console.SetCursorPosition(position.X + x, position.Y + y);
                    if (sprite.CharacterMap[x, y] == '.')
                    {
                        Console.Write(" ");
                    }
                    else if (sprite.CharacterMap[x, y] != ' ')
                    {
                        Console.Write(sprite.CharacterMap[x, y]);
                    }
                }
            }
        }

        static void ClearSprite(Sprite sprite, Point position)
        {
            for (int y = 0; y < sprite.Height; y++)
            {
                for (int x = 0; x < sprite.Width; x++)
                {
                    Console.SetCursorPosition(position.X + x, position.Y + y);
                    if (sprite.CharacterMap[x, y] != '.' && sprite.CharacterMap[x, y] != ' ')
                    {
                        Console.Write(" ");
                    }
                }
            }
        }

        static bool AreSpritesColliding(Sprite sprite1, Point spritePosition1, Sprite sprite2, Point spritePosition2)
        {
            for (int x = 0; x < sprite1.Width; x++)
            {
                for (int y = 0; y < sprite1.Height; y++)
                {
                    if (sprite1.CharacterMap[x, y] != ' ')
                    {
                        Point characterPoint = new Point(spritePosition1.X + x, spritePosition1.Y + y);

                        if (IsCollidingWithSprite(characterPoint, sprite2, spritePosition2))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        static bool IsCollidingWithSprite(Point globalPoint, Sprite sprite, Point spritePosition)
        {
            Point localPoint = new Point(globalPoint.X - spritePosition.X, globalPoint.Y - spritePosition.Y);

            if (localPoint.X < sprite.Width && localPoint.Y < sprite.Height && localPoint.X >= 0 && localPoint.Y >= 0)
            {
                return sprite.CharacterMap[localPoint.X, localPoint.Y] != ' ';
            }
            else
            {
                return false;
            }
        }

        static void DrawPlayer()
        {
            if (!Player.IsAttacking)
            {
                ConsoleColor? color = Player.IsAttacked ? ConsoleColor.DarkRed : null;

                DrawSpriteCombat(Player.Sprite, Player.Position, color);
                DrawSpriteCombat(CurrentWeapon.IdleSprite, Player.Position, color);
            }
            else
            {
                DrawSpriteCombat(Player.Sprite, Player.Position);
                DrawSpriteCombat(CurrentWeapon.AttackSprite, Player.Position);
            }

        }

        static void ClearPlayer()
        {
            Sprite activeEnemySprite = GetActiveEnemySprite();
            Sprite activeWeaponSprite = GetActiveWeaponSprite();

            ClearSprite(Player.Sprite, Player.Position);
            ClearSprite(activeWeaponSprite, Player.Position);

            if (IsPlayerCollidingWithEnemySprite())
            {
                DrawEnemy();
            }
        }

        static void DrawEnemy()
        {
            if (!CurrentEnemy.IsAttacking)
            {
                ConsoleColor? color = CurrentEnemy.IsAttacked ? ConsoleColor.DarkBlue : null;

                DrawSpriteCombat(CurrentEnemy.IdleSprite, CurrentEnemy.Position, color);
            }
            else
            {
                DrawSpriteCombat(CurrentEnemy.AttackSprites[CurrentEnemy.CurrentAttackIndex], CurrentEnemy.Position);
            }
        }

        static void ClearEnemy()
        {
            Sprite activeEnemySprite = GetActiveEnemySprite();
            Sprite activeWeaponSprite = GetActiveWeaponSprite();

            ClearSprite(activeEnemySprite, CurrentEnemy.Position);

            if (IsPlayerCollidingWithEnemySprite())
            {
                DrawPlayer();
            }
        }

        #endregion

        #region Assistance Methods

        static Sprite GetActiveEnemySprite()
        {
            return CurrentEnemy.IsAttacking ? CurrentEnemy.AttackSprites[CurrentEnemy.CurrentAttackIndex] : CurrentEnemy.IdleSprite;
        }

        static Sprite GetActiveWeaponSprite()
        {
            return Player.IsAttacking ? CurrentWeapon.AttackSprite : CurrentWeapon.IdleSprite;
        }

        static bool IsPlayerBodyCollidingWithEnemySprite()
        {
            return AreSpritesColliding(Player.Sprite, Player.Position, GetActiveEnemySprite(), CurrentEnemy.Position);
        }

        static bool IsWeaponColldingWithEnemySprite()
        {
            return AreSpritesColliding(GetActiveWeaponSprite(), Player.Position, GetActiveEnemySprite(), CurrentEnemy.Position);
        }

        static bool IsPlayerCollidingWithEnemySprite()
        {
            return IsPlayerBodyCollidingWithEnemySprite() || IsWeaponColldingWithEnemySprite();
        }

        #endregion

        #region AI


        static void SkeletonAI()
        {
            //Enemy Movement & Attack
            int enemyDirection = random.Next(37, 42);
            ConsoleKey enemyAI = (ConsoleKey)enemyDirection;

            bool canMove = !CurrentEnemy.IsAttacked && !CurrentEnemy.IsAttacking;

            if (canMove)
            {
                if (enemyAI == ConsoleKey.RightArrow && CurrentEnemy.Position.X < ArenaWidth - Skeleton.IdleSprite.Width)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.X++;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.LeftArrow && CurrentEnemy.Position.X > 0)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.X--;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.UpArrow && CurrentEnemy.Position.Y > 7)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.Y--;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.DownArrow && CurrentEnemy.Position.Y < ArenaHeight - Skeleton.IdleSprite.Height)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.Y++;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.Select)
                {
                    ClearEnemy();
                    CurrentEnemy.IsAttacking = true;
                    DrawEnemy();
                }
            }
        }

        static void SeaweedAI()
        {
            int attacking = random.Next(3);

            bool canMove = !CurrentEnemy.IsAttacked && !CurrentEnemy.IsAttacking;

            if (canMove)
            {
                if (attacking == 0)
                {
                    ClearEnemy();
                    CurrentEnemy.IsAttacking = true;
                    DrawEnemy();
                }
            }
        }

        static void BatAI()
        {
            //Movement
            int enemyDirection = random.Next(37, 41);
            ConsoleKey enemyAI = (ConsoleKey)enemyDirection;

            bool canMove = !CurrentEnemy.IsAttacked && !CurrentEnemy.IsAttacking;

            if (canMove)
            {
                if (enemyAI == ConsoleKey.RightArrow && CurrentEnemy.Position.X < ArenaWidth - Bat.IdleSprite.Width)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.X++;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.LeftArrow && CurrentEnemy.Position.X > 0)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.X--;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.UpArrow && CurrentEnemy.Position.Y > 7)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.Y--;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.DownArrow && CurrentEnemy.Position.Y < ArenaHeight - Bat.IdleSprite.Height)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.Y++;
                    DrawEnemy();
                }
            }
        }

        static void MimicAI()
        {
            //Movement
            int enemyDirection = random.Next(37, 42);
            ConsoleKey enemyAI = (ConsoleKey)enemyDirection;

            bool canMove = !CurrentEnemy.IsAttacked && !CurrentEnemy.IsAttacking;

            if (canMove)
            {
                if (enemyAI == ConsoleKey.RightArrow && CurrentEnemy.Position.X < ArenaWidth - Mimic.IdleSprite.Width)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.X++;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.LeftArrow && CurrentEnemy.Position.X > 0)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.X--;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.UpArrow && CurrentEnemy.Position.Y > 7)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.Y--;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.DownArrow && CurrentEnemy.Position.Y < ArenaHeight - Mimic.IdleSprite.Height)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.Y++;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.Select)
                {
                    ClearEnemy();
                    CurrentEnemy.IsAttacking = true;
                    DrawEnemy();
                }
            }
        }

        static void GolemAI()
        {
            int attacking = random.Next(7);

            bool canMove = !CurrentEnemy.IsAttacked && !CurrentEnemy.IsAttacking;

            if (canMove)
            {
                if (attacking == 0)
                {
                    ClearEnemy();
                    CurrentEnemy.CurrentAttackIndex = 0;
                    CurrentEnemy.IsAttacking = true;
                    DrawEnemy();
                }
                if (attacking == 3 && StoneGolem.HP < 1000)
                {
                    ClearEnemy();
                    CurrentEnemy.CurrentAttackIndex = 1;
                    CurrentEnemy.IsAttacking = true;
                    DrawEnemy();
                }
                if (attacking == 2 && StoneGolem.HP < 500)
                {
                    ClearEnemy();
                    CurrentEnemy.CurrentAttackIndex = 2;
                    CurrentEnemy.IsAttacking = true;
                    DrawEnemy();
                }
            }
        }

        static void GiantCrabAI()
        {
            //Movement
            int enemyDirection = random.Next(37, 42);
            ConsoleKey enemyAI = (ConsoleKey)enemyDirection;

            bool canMove = !CurrentEnemy.IsAttacked && !CurrentEnemy.IsAttacking;

            if (canMove)
            {
                if (enemyAI == ConsoleKey.RightArrow && CurrentEnemy.Position.X < ArenaWidth - GiantCrab.IdleSprite.Width)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.X++;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.LeftArrow && CurrentEnemy.Position.X > 0)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.X--;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.UpArrow && CurrentEnemy.Position.Y > 7)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.Y--;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.DownArrow && CurrentEnemy.Position.Y < ArenaHeight - GiantCrab.IdleSprite.Height)
                {
                    ClearEnemy();
                    CurrentEnemy.Position.Y++;
                    DrawEnemy();
                }
                else if (enemyAI == ConsoleKey.Select)
                {
                    int attackChance = random.Next(15);
                    if (attackChance == 0)
                    {
                        ClearEnemy();
                        CurrentEnemy.IsAttacking = true;
                        CurrentEnemy.CurrentAttackIndex = 2;
                        DrawEnemy();
                    }
                    else
                    {
                        ClearEnemy();
                        CurrentEnemy.IsAttacking = true;
                        int claw = random.Next(2);
                        if (claw == 0)
                        {
                            CurrentEnemy.CurrentAttackIndex = 0;
                            DrawEnemy();
                        }
                        else
                        {
                            CurrentEnemy.CurrentAttackIndex = 1;
                            DrawEnemy();
                        }
                    }
                }
            }
        }

        #endregion

        //Damage Direction Method
        static void DamageDirection()
        {
            if (Player.IsAttacking && !CurrentEnemy.IsAttacking && CurrentEnemy.IsAttacked && IsPlayerCollidingWithEnemySprite())
            {
                CurrentEnemy.HP = CurrentEnemy.HP - CurrentWeapon.Damage;
                DrawCombat();
            }
            else if (!Player.IsAttacking && Player.IsAttacked && CurrentEnemy.IsAttacking && IsPlayerCollidingWithEnemySprite())
            {
                Player.HP = Player.HP - CurrentEnemy.Damage;
                DrawCombat();
            }
            else if (!Player.IsAttacking && !Player.IsAttacked && CurrentEnemy.IsAttacking && IsPlayerCollidingWithEnemySprite())
            {
                Player.HP = Player.HP - CurrentEnemy.Damage;
                DrawCombat();
            }
            else if (!Player.IsAttacking && !Player.IsAttacked && !CurrentEnemy.IsAttacking && IsPlayerBodyCollidingWithEnemySprite())
            {
                Player.HP = Player.HP - CurrentEnemy.Damage;
                DrawCombat();
            }
        }

        static bool MainCombat()
        {
            //Initialize All Objects
            InitializeArena();
            InitializeEnemies();
            InitializePlayer();
            InitializeWeapons();

            //Prepare Console
            Console.CursorVisible = false;
            Console.SetWindowSize(ArenaWidth, ArenaHeight);
            Console.SetBufferSize(ArenaWidth, ArenaHeight);
            Console.BackgroundColor = ArenaBackgroundColor;
            Console.Clear();

            //Initialize Combat
            Player.Position = new Point(20, ArenaHeight / 2 - Player.Sprite.Height / 2);
            CurrentWeapon = Cutlass;
            CurrentWeapon.Position = Player.Position;

            if (OnTreasureIsland)
            {
                CurrentEnemy = GiantCrab;
            }
            else
            {
                int currentEnemyRoll = random.Next(0, 100);

                if (currentEnemyRoll <= 30)
                {
                    CurrentEnemy = Skeleton;
                }
                else if (currentEnemyRoll <= 60 && currentEnemyRoll >= 31)
                {
                    CurrentEnemy = Bat;
                }
                else if (currentEnemyRoll <= 80 && currentEnemyRoll >= 61)
                {
                    CurrentEnemy = Seaweed;
                }
                else if (currentEnemyRoll <= 95 && currentEnemyRoll >= 81)
                {
                    CurrentEnemy = Mimic;
                }
                else if (currentEnemyRoll >= 96)
                {
                    CurrentEnemy = StoneGolem;
                }
            }



            CurrentEnemy.Position = new Point(80, ArenaHeight / 2 - CurrentEnemy.IdleSprite.Height / 2);

            DrawCombat();

            //Cooldowns
            TimeSpan playerTimer = TimeSpan.Zero;
            TimeSpan enemyTimer = TimeSpan.Zero;
            Stopwatch frameStopwatch = Stopwatch.StartNew();
            frameStopwatch.Restart();

            //Game Loop
            while (true)
            {

                //Handle Inputs
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    ConsoleKey pressedKey = keyInfo.Key;

                    bool canMove = !Player.IsAttacked && !Player.IsAttacking;

                    if (canMove)
                    {
                        //Handle Weapons Cycling
                        if (pressedKey == ConsoleKey.C)
                        {
                            ClearPlayer();
                            if (CurrentWeapon == Cutlass)
                            {
                                CurrentWeapon = Dagger;
                            }
                            else if (CurrentWeapon == Dagger)
                            {
                                CurrentWeapon = BoatAxe;
                            }
                            else if (CurrentWeapon == BoatAxe)
                            {
                                CurrentWeapon = Flintlock;
                            }
                            else if (CurrentWeapon == Flintlock)
                            {
                                CurrentWeapon = Cutlass;
                            }
                            DrawPlayer();
                            DrawCombat();
                        }

                        //Attack With Weapons
                        if (pressedKey == ConsoleKey.V && CurrentWeapon.Ammo > 0)
                        {
                            ClearPlayer();
                            Player.IsAttacking = true;
                            DrawPlayer();
                            playerTimer = CurrentWeapon.AttackTime;
                            if (CurrentWeapon == Flintlock && Flintlock.Ammo > 0)
                            {
                                Flintlock.Ammo--;
                            }
                            DrawCombat();
                        }

                        //Handle Player Movements

                        if (pressedKey == ConsoleKey.LeftArrow && Player.Position.X > 0)
                        {
                            ClearPlayer();
                            Player.Position.X -= 1;
                            DrawPlayer();
                        }
                        else if (pressedKey == ConsoleKey.RightArrow && Player.Position.X < ArenaWidth - 6)
                        {
                            ClearPlayer();
                            Player.Position.X += 1;
                            DrawPlayer();
                        }
                        else if (pressedKey == ConsoleKey.UpArrow && Player.Position.Y > 7)
                        {
                            ClearPlayer();
                            Player.Position.Y -= 1;
                            DrawPlayer();
                        }
                        else if (pressedKey == ConsoleKey.DownArrow && Player.Position.Y < ArenaHeight - 4)
                        {
                            ClearPlayer();
                            Player.Position.Y += 1;
                            DrawPlayer();
                        }
                        DamageDirection();
                    }

                    //Exit Game
                    if (pressedKey == ConsoleKey.Escape)
                    {
                        return false;
                    }
                }

                //Update Game Objects
                long elapsedTicks = frameStopwatch.ElapsedTicks;
                TimeSpan elapsed = new TimeSpan(elapsedTicks);
                frameStopwatch.Restart();


                //Collision Detection
                if (!Player.IsAttacking && !Player.IsAttacked && IsPlayerBodyCollidingWithEnemySprite())
                {
                    ClearPlayer();
                    Player.IsAttacked = true;
                    playerTimer = DamageCooldown;
                    Player.Position.X -= 5;
                    DrawPlayer();
                    DamageDirection();
                    DrawCombat();
                }
                else if (Player.IsAttacking && !CurrentEnemy.IsAttacking && !CurrentEnemy.IsAttacked && IsWeaponColldingWithEnemySprite())
                {
                    ClearEnemy();
                    CurrentEnemy.IsAttacked = true;
                    enemyTimer = DamageCooldown;
                    DrawEnemy();
                    DamageDirection();
                    DrawCombat();
                }

                //Handle Damage Cooldown
                if (Player.IsAttacked)
                {
                    playerTimer -= elapsed;

                    //End Attack At End of Cooldown
                    if (playerTimer <= TimeSpan.Zero)
                    {
                        ClearPlayer();
                        Player.IsAttacked = false;
                        DrawPlayer();
                    }
                }


                //Handle Weapon Cooldown
                if (Player.IsAttacking)
                {
                    playerTimer -= elapsed;

                    //End Attack At End of Cooldown
                    if (playerTimer <= TimeSpan.Zero)
                    {
                        ClearPlayer();
                        Player.IsAttacking = false;
                        DrawPlayer();
                    }
                }

                //Handle Enemies
                enemyTimer -= elapsed;
                if (enemyTimer <= TimeSpan.Zero)
                {
                    //Restart Enemy Timer
                    if (!CurrentEnemy.IsAttacking)
                    {
                        enemyTimer = CurrentEnemy.MovementTime;
                    }
                    else
                    {
                        enemyTimer = CurrentEnemy.AttackTime;
                    }

                    //Reset Enemy
                    ClearEnemy();
                    CurrentEnemy.IsAttacking = false;
                    CurrentEnemy.IsAttacked = false;
                    DrawEnemy();

                    //Call Enemy Script / Behavior
                    if (CurrentEnemy == Skeleton)
                    {
                        SkeletonAI();
                    }
                    else if (CurrentEnemy == Seaweed)
                    {
                        SeaweedAI();
                    }
                    else if (CurrentEnemy == Bat)
                    {
                        BatAI();
                    }
                    else if (CurrentEnemy == Mimic)
                    {
                        MimicAI();
                    }
                    else if (CurrentEnemy == StoneGolem)
                    {
                        GolemAI();
                    }
                    else if (CurrentEnemy == GiantCrab)
                    {
                        GiantCrabAI();
                    }
                }

                if (Player.HP <= 0)
                {
                    break;
                }
                else if (CurrentEnemy.HP <= 0)
                {
                    break;
                }
            }

            if (Player.HP <= 0)
            {
                return false;
            }
            else
            {
                if (CurrentEnemy != GiantCrab)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("YARRRGH! You have emerged victorious! You have recieved a clue for your brave actions during combat!");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("YARRRGH! You have slain King Crab, the Guardian of the Treasure! Victory is yours!");
                }
                Console.ReadKey(true);
                return true;
            }
        }
    }
}