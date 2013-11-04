using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace One_Man_Army
{
    // TODO: Edit collision code to find exact point of intersection.

    public struct BulletCollisionCircle
    {
        public Circle circle;
        public float damage;
    }

    public enum BulletMovementType
    {
        Straight = 0,
        ParabolicArc = 1,
        Homing = 2
    }

    public class Bullet
    {
        #region Fields

        Vector2 position;
        Vector2 lastPosition;
        Vector2 velocity;
        Vector2 origin;
        float speed;
        float rotation;
        float damage;
        float damageRadius;
        Circle collisionCircle;
        BulletMovementType movementType;
        Texture2D texture;
        SmokeEmitterInstance smokeTrail;
        bool friendly;
        Enemy enemyToHomeOn;
        bool hasSmokeTrail;

        const float accelerationFromGravity = 500f;

        /// <summary>
        /// The bullet's position  
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }

        /// <summary>
        /// The bullet's position last frame
        /// </summary>
        public Vector2 LastPosition
        {
            get { return lastPosition; }
            set { lastPosition = position + value * 0; }
        }

        /// <summary>
        /// The bullet's velocity
        /// </summary>
        public Vector2 Velocity
        {
            get { return velocity; }
        }

        /// <summary>
        /// The speed of the bullet
        /// </summary>
        public float Speed
        {
            get { return speed; }
        }

        /// <summary>
        /// The orientation of the bullet
        /// </summary>
        public float Rotation
        {
            get { return rotation; }
        }

        /// <summary>
        /// The amount of damage the bullet will inflict upon impact
        /// </summary>
        public float Damage
        {
            get { return damage; }
        }

        /// <summary>
        /// The radius of damage the bullet will inflict on impact
        /// </summary>
        public float DamageRadius
        {
            get { return damageRadius; }
        }

        /// <summary>
        /// The sphere used for collision detection with the bullet
        /// </summary>
        public Circle CollisionCircle
        {
            get { return collisionCircle; }
        }

        /// <summary>
        /// The movement type of the bullet (straight, parabolic arc, etc.)
        /// </summary>
        public BulletMovementType MovementType
        {
            get { return movementType; }
        }

        public SmokeEmitterInstance SmokeTrail
        {
            get { return smokeTrail; }
            set { smokeTrail = value; }
        }

        #endregion

        #region Initialization

        public Bullet()
        { }

        public void initBullet(Vector2 pos, Vector2 dir, float vel, float colRad, float dam, float damRad, 
            BulletMovementType moveType, Texture2D texture, bool isFriendly, bool hasSmokeTrail)
        {
            this.damage = dam;
            this.position = pos;
            this.speed = vel;
            this.velocity = dir * speed;
            this.collisionCircle = new Circle(position, colRad);
            this.damageRadius = damRad;
            this.movementType = moveType;
            this.texture = texture;
            this.origin = new Vector2(texture.Width / 2, texture.Height / 2);
            this.friendly = isFriendly;
            this.hasSmokeTrail = hasSmokeTrail;
            this.enemyToHomeOn = null;

            Vector2 direction = Vector2.Normalize(velocity);
            this.rotation = (float)Math.Atan2(direction.Y, direction.X);
        }

        #endregion

        #region Update

        public void Update(Level level, float elapsed)
        {
            GetVelocity(level, elapsed);
            
            lastPosition = position;
            rotation = (float)Math.Atan2(velocity.Y, velocity.X);
            position += velocity * elapsed;
            collisionCircle.Position = position;

            if (hasSmokeTrail)
            {
                smokeTrail.Position = this.Position - Vector2.Normalize(this.velocity) * this.collisionCircle.Radius * 2;
                smokeTrail.Radius = this.collisionCircle.Radius;
                smokeTrail.Update(elapsed);
            }
        }

        private void GetVelocity(Level level, float elapsed)
        {
            switch (movementType)
            {
                case BulletMovementType.Straight:

                    break;

                case BulletMovementType.ParabolicArc:

                    velocity.Y += accelerationFromGravity * elapsed;

                    break;

                case BulletMovementType.Homing:

                    if (!friendly)
                    {
                        velocity = One_Man_Army_Game.StepSlowlyToVector(elapsed, 250 + this.speed,
                            Vector2.Normalize(new Vector2(level.Player.Position.X - 
                                this.position.X, level.Player.Position.Y - this.Position.Y - 50)), this.velocity);
                        rotation = (float)Math.Atan2(velocity.Y, velocity.X);

                        velocity = Vector2.Normalize(velocity);
                        velocity *= speed;
                        break;
                    }

                    if (level.Enemies.Count > 0)
                    {
                        PickEnemyToHomeOn(level);
                        if (enemyToHomeOn != null && enemyToHomeOn.State == EnemyState.Alive)
                        {
                            velocity = One_Man_Army_Game.StepSlowlyToVector(elapsed, 250 + this.speed, 
                                Vector2.Normalize(enemyToHomeOn.Center - this.position), this.velocity);
                            rotation = (float)Math.Atan2(velocity.Y, velocity.X);
                        }

                        velocity = Vector2.Normalize(velocity);
                        velocity *= speed;
                    }

                    break;
            }
        }

        /// <summary>
        /// Decides whether the bullet should home in on an enemy, and figures out which one.
        /// </summary>
        private void PickEnemyToHomeOn(Level level)
        {
            if (enemyToHomeOn != null && enemyToHomeOn.State == EnemyState.Alive)
                return;
            
            float closestDist = float.PositiveInfinity;
            float currentDist;

            Dictionary<float, Enemy> enemyDictionary = new Dictionary<float, Enemy>();

            currentDist = Math.Abs(Vector2.Distance(this.position, level.Enemies[0].Center));

            // Finds the closest enemy.
            foreach (Enemy enemy in level.Enemies)
            {
                bool shouldAdd = true;

                currentDist = Math.Abs(Vector2.Distance(this.position, enemy.Center));
                closestDist = MathHelper.Min(closestDist, currentDist);

                foreach (float f in enemyDictionary.Keys)
                {
                    if (currentDist == f)
                        shouldAdd = false;
                }
                
                if (shouldAdd)
                    enemyDictionary.Add(currentDist, enemy);
            }

            if (closestDist > 500)
                return;

            List<float> sortedEnemyKeys = new List<float>();
            foreach (float f in enemyDictionary.Keys)
            {
                if (sortedEnemyKeys.Count == 0)
                {
                    sortedEnemyKeys.Add(f);
                    continue;
                }

                for (int i = 0; i < sortedEnemyKeys.Count; i++)
                {
                    if (f < sortedEnemyKeys[i])
                    {
                        sortedEnemyKeys.Insert(i, f);
                        break;
                    }
                }
            }

            foreach (float f in sortedEnemyKeys)
            {
                if (f > 500)
                    return;

                Vector2 bulletToEnemy = Vector2.Normalize(enemyDictionary[f].Position - this.Position);
                float angleBetweenVectors = (float)Math.Abs(
                    Math.Atan2(bulletToEnemy.Y, bulletToEnemy.X) -
                    Math.Atan2(this.Velocity.Y, this.Velocity.X));
                if (angleBetweenVectors > MathHelper.Pi)
                    angleBetweenVectors = MathHelper.Pi * 2 - angleBetweenVectors;

                if (angleBetweenVectors < MathHelper.PiOver2 * (1 - (2f / 3f) * (f / 500)))
                {
                    enemyToHomeOn = enemyDictionary[f];
                    return;
                }
            }
        }

        #endregion

        #region Collisions

        public BulletCollisionCircle CheckForCollisionsWithEnemies(Level level)
        {
            BulletCollisionCircle circle = new BulletCollisionCircle();

            if (CheckForCollisionsWithLevel(level))
            {
                circle.circle = new Circle(this.position, this.damageRadius);
                circle.damage = this.Damage;
                if (damageRadius > collisionCircle.Radius)
                    level.Screen.SFXManager.PlayCue((damageRadius > 24 ? "Large " : "Small ") + "Explosion");
                return circle;
            }

            foreach (Enemy enemy in level.Enemies)
            {
                if (Collisions.DoCollision(enemy.CollisionPolygon, this.collisionCircle))
                {
                    circle.circle = new Circle(this.position, this.damageRadius);
                    circle.damage = this.Damage;
                    if (damageRadius > collisionCircle.Radius)
                        level.Screen.SFXManager.PlayCue((damageRadius > 24 ? "Large " : "Small ") + "Explosion");
                    return circle;
                }
            }

            circle.circle = null;
            return circle;
        }

        public BulletCollisionCircle CheckForCollisionsWithPlayer(Level level)
        {
            BulletCollisionCircle circle = new BulletCollisionCircle();

            if (CheckForCollisionsWithLevel(level))
            {
                circle.circle = new Circle(this.position, this.damageRadius);
                circle.damage = this.Damage;
                if (damageRadius > collisionCircle.Radius)
                    level.Screen.SFXManager.PlayCue((damageRadius > 24 ? "Large " : "Small ") + "Explosion");
                return circle;
            }

            if (Collisions.DoCollision(level.Player.CollisionPolygon, this.collisionCircle))
            {
                circle.circle = new Circle(this.position, this.damageRadius);
                circle.damage = this.Damage;
                if (damageRadius > collisionCircle.Radius)
                    level.Screen.SFXManager.PlayCue((damageRadius > 24 ? "Large " : "Small ") + "Explosion");
                return circle;
            }

            circle.circle = null;
            return circle;
        }

        private bool CheckForCollisionsWithLevel(Level level)
        {
            int x = (int)level.GetTilePosition(this.Position).X;
            int y = (int)level.GetTilePosition(this.Position).Y;

            for (int a = x - 1; a < x + 1; a++)
            {
                for (int b = y - 1; b < y + 1; b++)
                {
                    TileCollision tile = level.GetCollision(a, b);

                    if (tile == TileCollision.Impassable ||
                        tile == TileCollision.SlantedUp ||
                        tile == TileCollision.SlantedDown)
                    {
                        if (Collisions.DoCollision(level.GetTilePolygon(a, b), this.collisionCircle))
                        {
                            return true;
                        }
                    }
                    if (tile == TileCollision.Platform)
                    {
                        if (Collisions.DoCollision(this.collisionCircle, level.GetTileBounds(a, b)) &&
                            Collisions.DoCollision(new Circle(this.lastPosition, this.collisionCircle.radius),
                                level.GetTileBounds(a, b - 1)) &&
                            this.velocity.Y > 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Draw

        public void Draw(SpriteBatch spriteBatch, bool red)
        {
            Color color = red ? Color.Red : (damageRadius > collisionCircle.radius ? Color.White : new Color(70, 70, 70));
            if (this.friendly)
                color = Color.White;

            spriteBatch.Draw(texture, position, null, color, rotation, origin, 1f, SpriteEffects.None, 0);
        }

        #endregion
    }
}
