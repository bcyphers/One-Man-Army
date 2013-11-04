using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DataTypeLibrary;

namespace One_Man_Army
{
    public enum PowerUpType
    {
        Ammo,
        HealthIncrease,
        HealthPack,
        Repair,
        Weapon
    }

    public class PowerUp
    {
        public PowerUpType Type;
        public Weapon Weapon;
        public Texture2D Texture;
        public Level Level;
        public Vector2 Position;
        public Vector2 Origin;
        public float VelocityY;

        const int Lifetime = 30;

        private float lifeRemaining = Lifetime;
        private float previousBottom;
        private bool isOnGround;
        private Rectangle localBounds;

        public bool IsAlive
        {
            get { return lifeRemaining > 0; }
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - Origin.X);
                int top = (int)Math.Round(Position.Y - Origin.Y);

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        public PowerUp() { }

        public PowerUp(PowerUpType type, Weapon wep, Vector2 pos, Level level, Texture2D tex, bool onGround)
        {
            Type = type;
            if (type == PowerUpType.Weapon)
                Weapon = wep;
            Level = level;
            Texture = tex;
            Position = pos;
            previousBottom = Position.Y;
            localBounds = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Origin = new Vector2(Texture.Width / 2, Texture.Height);
            isOnGround = onGround;
        }


        /// <summary>
        /// Updates the power-up while it is active in the level.
        /// </summary>
        public void Update(float elapsed)
        {
            lifeRemaining -= elapsed;

            float GravityAcceleration = 500.0f;
            float MaxFallSpeed = 1000.0f;

            VelocityY = MathHelper.Clamp(VelocityY + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            Position.Y += VelocityY * elapsed;

            HandleCollisions();
        }

        /// <summary>
        /// Detects and resolves collisions between the power-up and the level geometry.
        /// </summary>
        private void HandleCollisions()
        {
            int x = (int)Level.GetTilePosition(Position).X;
            int y = (int)Level.GetTilePosition(Position).Y;

            TileCollision collision = Level.GetCollision(x, y);

            if (collision == TileCollision.SlantedUp || collision == TileCollision.SlantedDown)
            {
                // If the power-up is actually on the ramp
                if (Collisions.DoCollision(Level.GetTilePolygon(x, y), BoundingRectangle))
                {
                    // The power-up's center must be on the tile, not just a corner.
                    if (collision == TileCollision.SlantedUp)
                    {
                        // The power-up's Y position is a linear interpolation of the power-up's X position
                        // relative to the tile.
                        float relativeY = Position.X - x * Tile.Width;
                        relativeY /= Tile.Width;
                        Position.Y = MathHelper.Lerp(y * Tile.Height, (y + 1) * Tile.Height, 1 - relativeY) - 1;

                        isOnGround = true;
                        VelocityY = 0;
                    }
                    if (collision == TileCollision.SlantedDown)
                    {
                        // The power-up's Y position is a linear interpolation of the power-up's X position
                        // relative to the tile.
                        float relativeY = Position.X - x * Tile.Width;
                        relativeY /= Tile.Width;
                        Position.Y = MathHelper.Lerp(y * Tile.Height, (y + 1) * Tile.Height, relativeY) - 1;

                        isOnGround = true;
                        VelocityY = 0;
                    }
                }
            }
            else if (collision != TileCollision.Passable)
            {
                Position.Y = Level.GetTileBounds(x, y).Top;
                isOnGround = true;
                VelocityY = 0;
            }
        }

        public void Activate()
        {
        }

        /// <summary>
        /// Draws the weapon's sprite at its location.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, 0, Origin,
                1f, SpriteEffects.None, 0);
        }
    }
}
