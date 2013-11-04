using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace One_Man_Army
{
    /// <summary>
    /// Controls the collision detection and response behavior of a tile.
    /// </summary>
    public enum TileCollision
    {
        /// <summary>
        /// A passable tile is one which does not hinder player motion at all.
        /// </summary>
        Passable = 0,

        /// <summary>
        /// An impassable tile is one which does not allow the player to move through
        /// it at all. It is completely solid.
        /// </summary>
        Impassable = 1,

        /// <summary>
        /// A platform tile is one which behaves like a passable tile except when the
        /// player is above it. A player can jump up through a platform as well as move
        /// past it to the left and right, but can not fall down through the top of it.
        /// </summary>
        Platform = 2,

        /// <summary>
        /// An exit tile acts as passable most of the time, but will allow the player to
        /// continue on to the next level if activated.
        /// </summary>
        Exit = 3,

        /// <summary>
        /// Slanted tiles act as triangles, giving the player a ramp to walk up or down.
        /// </summary>
        SlantedUp = 4,

        SlantedDown = 5
    }

    /// <summary>
    /// Enum representing the four edges of a tile. Useful for determining which edges to 
    /// check for collisions with.
    /// </summary>
    public enum TileEdge
    {
        Left = 0,

        Top = 1,

        Right = 2,

        Bottom = 3
    }

    /// <summary>
    /// Stores the appearance and collision behavior of a tile.
    /// </summary>
    public struct Tile
    {
        #region Fields

        Texture2D texture;
        public Texture2D Texture
        {
            get { return texture; }
        }

        Texture2D damagedTexture;
        public Texture2D DamagedTexture
        {
            get { return damagedTexture; }
        }

        TileCollision collision;
        public TileCollision Collision
        {
            get { return collision; }
        }

        public const int Width = 64;
        public const int Height = 48;

        bool destructible;
        public bool Destructible
        {
            get { return destructible; }
        }

        float health;
        public float Health
        {
            get { return health; }
        }

        public int TileToDraw;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        #endregion

        #region Methods

        /// <summary>
        /// Constructs a new tile.
        /// </summary>
        public Tile(Texture2D texture, Texture2D damagedTexture, TileCollision collision, bool destruct)
        {
            this.texture = texture;
            this.collision = collision;
            this.destructible = destruct;
            this.damagedTexture = damagedTexture;
            if (destructible)
                health = 1f;
            else
                health = 0;
            TileToDraw = 0;
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0 && Destructible)
                this = new Tile(null, null, TileCollision.Passable, false);
        }

        #endregion
    }
}
