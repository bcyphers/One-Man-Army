using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using DataTypeLibrary;

namespace One_Man_Army
{
    public static class LevelLoader
    {
        private static Level level;

        #region Public Methods

        public static void Reset()
        {
            level = null;
        }

        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, etc.
        /// </summary>
        /// <param name="path">
        /// The absolute path to the level file to be loaded.
        /// </param>
        public static void LoadTiles(Level l, ref Tile[,] tiles, string path)
        {
            if (level == null)
                level = l;

            // Load the level from a 2D color array.
            Texture2D levelTex = level.Content.Load<Texture2D>(path);

            Color[,] bitmap = TextureTo2DArray(levelTex);

            // Allocate the tile grid.
            tiles = new Tile[bitmap.GetLength(0), bitmap.GetLength(1)];

            int Height = tiles.GetLength(1);
            int Width = tiles.GetLength(0);

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    Color tileColor = bitmap[x, y];
                    tiles[x, y] = LoadTile(tileColor, x, y);
                }
            }

            level.LevelBounds = new Rectangle(0, 0, Width * Tile.Width, Height * Tile.Height);

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    if (tiles[x, y].Collision == TileCollision.Impassable)
                        FindNonCollidableEdges(x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (level.Player == null)
                throw new NotSupportedException("A level must have a starting point.");
        }

        /// <summary>
        /// Loads a particular tank's sprite sheet and sounds.
        /// </summary>
        public static TankDataHolder LoadTankData(string spriteSet)
        {
            TankData data = level.Content.Load<TankData>("Enemy Data/" + spriteSet);
            TankDataHolder dataHold = new TankDataHolder(data);

            // Load animations.
            dataHold.RunAnimation = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/" + "Run");
            dataHold.IdleAnimation = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/" + "Idle");

            // Load the tank's weapon
            dataHold.Weapon = level.Content.Load<WeaponData>("Weapons/" + spriteSet);

            dataHold.BulletTexture = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/Weapon/bullet");
            dataHold.FireTextParticle = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/Weapon/sound");

            dataHold.WeaponFire = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/Weapon/fire");
            dataHold.WeaponIdle = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/Weapon/idle");

            return dataHold;
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public static HeliDataHolder LoadHeliData(string spriteSet)
        {
            HeliData data = level.Content.Load<HeliData>("Enemy Data/" + spriteSet);
            HeliDataHolder dataHold = new HeliDataHolder(data);

            // Load animations.
            dataHold.MoveAnimation = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/" + "Base");

            // Load the tank's weapon
            dataHold.Weapon = level.Content.Load<WeaponData>("Weapons/" + spriteSet);

            dataHold.BulletTexture = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/Weapon/bullet");
            dataHold.FireTextParticle = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/Weapon/sound");

            dataHold.WeaponFire = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/Weapon/fire");
            dataHold.WeaponIdle = level.Content.Load<Texture2D>("Sprites/Enemies/" + spriteSet + "/Weapon/idle");

            return dataHold;
        }

        /// <summary>
        /// This mess of if... elses and switch statements finds which edge of tile[x, y] 
        /// are collidable, thus determining which part of the tile sprite should be drawn.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void FindNonCollidableEdges(int x, int y)
        {
            List<TileEdge> NonCollidableEdges = new List<TileEdge>();
            Tile[,] tiles = level.Tiles;

            if (level.GetCollision(x - 1, y) == TileCollision.Impassable ||
                level.GetCollision(x - 1, y) == TileCollision.SlantedUp)
                NonCollidableEdges.Add(TileEdge.Left);
            if (level.GetCollision(x + 1, y) == TileCollision.Impassable ||
                level.GetCollision(x + 1, y) == TileCollision.SlantedDown)
                NonCollidableEdges.Add(TileEdge.Right);
            if (level.GetCollision(x, y - 1) == TileCollision.Impassable ||
                level.GetCollision(x, y - 1) == TileCollision.SlantedUp ||
                level.GetCollision(x, y - 1) == TileCollision.SlantedDown)
                NonCollidableEdges.Add(TileEdge.Top);
            if (level.GetCollision(x, y + 1) == TileCollision.Impassable)
                NonCollidableEdges.Add(TileEdge.Bottom);

            if (NonCollidableEdges.Count == 0)
            {
                tiles[x, y].TileToDraw = 0;
                return;
            }

            if (NonCollidableEdges.Count == 4)
            {
                tiles[x, y].TileToDraw = 15;
                return;
            }

            switch (NonCollidableEdges[0])
            {
                case TileEdge.Left:

                    if (NonCollidableEdges.Count == 1)
                    {
                        tiles[x, y].TileToDraw = 1;
                    }
                    else
                    {
                        if (NonCollidableEdges.Count == 2)
                        {
                            switch (NonCollidableEdges[1])
                            {
                                case TileEdge.Right:
                                    tiles[x, y].TileToDraw = 5;
                                    break;
                                case TileEdge.Top:
                                    tiles[x, y].TileToDraw = 6;
                                    break;
                                case TileEdge.Bottom:
                                    tiles[x, y].TileToDraw = 7;
                                    break;
                            }
                        }
                        else
                        {
                            if (NonCollidableEdges[1] == TileEdge.Right)
                            {
                                if (NonCollidableEdges[2] == TileEdge.Top)
                                    tiles[x, y].TileToDraw = 13;
                                else
                                    tiles[x, y].TileToDraw = 14;
                            }
                            else
                            {
                                tiles[x, y].TileToDraw = 11;
                            }
                        }
                    }
                    break;

                case TileEdge.Right:

                    if (NonCollidableEdges.Count == 1)
                    {
                        tiles[x, y].TileToDraw = 2;
                    }
                    else
                    {
                        if (NonCollidableEdges.Count == 2)
                        {
                            if (NonCollidableEdges[1] == TileEdge.Top)
                                tiles[x, y].TileToDraw = 8;
                            else
                                tiles[x, y].TileToDraw = 9;
                        }
                        else
                        {
                            tiles[x, y].TileToDraw = 12;
                        }
                    }
                    break;

                case TileEdge.Top:

                    if (NonCollidableEdges.Count == 1)
                        tiles[x, y].TileToDraw = 3;
                    else
                        tiles[x, y].TileToDraw = 10;
                    break;

                case TileEdge.Bottom:

                    tiles[x, y].TileToDraw = 4;
                    break;
            }

            return;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates the texture data array for the tilemap.
        /// </summary>
        private static Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];

            return colors2D;
        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private static Tile LoadTile(Color tileType, int x, int y)
        {
            const string passable = "{R:255 G:255 B:255 A:255}";
            const string platform = "{R:220 G:220 B:220 A:255}";
            const string destructibleBlock = "{R:70 G:70 B:70 A:255}";
            const string staticBlock = "{R:0 G:0 B:0 A:255}";
            const string slantedUp = "{R:100 G:0 B:0 A:255}";
            const string slantedDown = "{R:0 G:0 B:100 A:255}";
            const string playerSpawn = "{R:0 G:0 B:0 A:0}";
            const string heliSpawn = "{R:255 G:255 B:0 A:255}";
            const string tankSpawn = "{R:0 G:255 B:255 A:255}";

            switch (tileType.ToString())
            {
                // Blank space
                case passable:
                    return new Tile(null, null, TileCollision.Passable, false);

                // Floating platform
                case platform:
                    return LoadVarietyTile("BlockB", 2, TileCollision.Platform, true);

                // Floating block
                case destructibleBlock:
                    return LoadVarietyTile("BlockA", 4, TileCollision.Impassable, true);

                // Impassable block
                case staticBlock:
                    return LoadVarietyTile("BlockA", 1, TileCollision.Impassable, false);

                // Block slanted up
                case slantedUp:
                    return LoadSlantedTile("SlantA", true);

                // Block slanted down.
                case slantedDown:
                    return LoadSlantedTile("SlantA", false);

                // Player 1 start point
                case playerSpawn:
                    return LoadStartTile(x, y);

                // Various enemies
                case heliSpawn:
                    return LoadEnemySpawnTile(x, y, "Heli");
                case tankSpawn:
                    return LoadEnemySpawnTile(x, y, "Tank");

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format(
                        "Unsupported tile type color '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private static Tile LoadTile(string name, TileCollision collision, bool destruct)
        {
            string loadString = collision == TileCollision.Impassable ?
                String.Format("Tiles/{0}/{0}", name) : "Tiles/" + name;

            Texture2D damaged = destruct ? level.Content.Load<Texture2D>(loadString + "D") :
                null;

            return new Tile(level.Content.Load<Texture2D>(loadString), damaged, collision, destruct);
        }

        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private static Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision, bool destruct)
        {
            int index = level.Random.Next(variationCount);

            string loadString = collision == TileCollision.Impassable ?
                String.Format("Tiles/{0}/{0}", baseName) : "Tiles/" + baseName;

            Texture2D damaged = destruct ? level.Content.Load<Texture2D>(loadString + "D/" + baseName + "D" + index) :
                null;

            return new Tile(level.Content.Load<Texture2D>(loadString), damaged, collision, destruct);
        }

        /// <summary>
        /// Loads a ramp, either up or downward facing.
        /// </summary>
        /// <param name="upOrDown"></param>
        /// <returns></returns>
        private static Tile LoadSlantedTile(string name, bool upOrDown)
        {
            TileCollision collision = upOrDown ? TileCollision.SlantedUp : TileCollision.SlantedDown;
            name = upOrDown ? name + "U" : name + "D";
            return LoadTile(name, collision, false);
        }

        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private static Tile LoadStartTile(int x, int y)
        {
            if (level.Player != null && level.Player.IsAlive)
                return new Tile(null, null, TileCollision.Passable, false);;

            level.Start = RectangleExtensions.GetBottomCenter(level.GetTileBounds(x, y));
            List<Weapon> weps = new List<Weapon>();

            if (level.Player != null)
                weps = level.Player.Weapons;
            else
                weps.Add(level.Game.AllWeapons["Pistol"]);

            level.Player = new Player(level, level.Start, weps);

            return new Tile(null, null, TileCollision.Passable, false);
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private static Tile LoadEnemySpawnTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(level.GetTileBounds(x, y));

            Type t = null;

            if (spriteSet.StartsWith("Tank"))
                t = typeof(Tank);
            if (spriteSet.StartsWith("Heli"))
                t = typeof(Heli);

            level.EnemySpawnPoints.Add(new EnemySpawnPoint(position, spriteSet, t));

            return new Tile(null, null, TileCollision.Passable, false);
        }

        #endregion
    }
}
