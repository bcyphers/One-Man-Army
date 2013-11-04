using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using DataTypeLibrary;

namespace One_Man_Army
{
    public struct HeliDataHolder
    {
        public HeliDataHolder(HeliData data)
        {
            Acceleration = data.Acceleration;
            MaxHealth = data.MaxHealth;
            MaxMoveSpeed = data.MaxMoveSpeed;
            MaxVerticalSpeed = data.MaxVerticalSpeed;
            Polygon = data.Polygon;
            MaxOverheatTime = data.MaxOverheatTime;
            OverheatFactor = data.OverheatFactor;

            MoveAnimation = null;
            WeaponFire = null;
            WeaponIdle = null;
            BulletTexture = null;
            FireTextParticle = null;

            Weapon = null;
        }

        public float Acceleration;
        public float MaxHealth;
        public float MaxMoveSpeed;
        public float MaxVerticalSpeed;
        public List<Vector2> Polygon;
        public float MaxOverheatTime;
        public float OverheatFactor;

        public Texture2D MoveAnimation;
        public Texture2D WeaponFire;
        public Texture2D WeaponIdle;
        public Texture2D BulletTexture;
        public Texture2D FireTextParticle;

        public WeaponData Weapon;
    }

    public class Heli : Enemy
    {
        #region Fields

        public bool IsFiring
        {
            get { return isFiring; }
        }
        private bool isFiring = false;

        public Vector2 Velocity
        {
            get { return velocity; }
        }
        Vector2 velocity;

        public float Rotation
        {
            get
            {
                rotation = Velocity.X / MaxMoveSpeed * MathHelper.PiOver4 / 2;
                return rotation;
            }
        }
        float rotation = 0f;

        public Vector2 CannonDirection
        {
            get { return cannonDirection; }
            set { cannonDirection = value; }
        }
        Vector2 cannonDirection;

        public Vector2 CannonPosition
        {
            get { return cannonPosition; }
        }
        Vector2 cannonPosition;

        Vector2 directionToPlayer;

        public float CannonRotation
        {
            get
            {
                cannonRotation = (float)Math.Atan2(cannonDirection.Y, cannonDirection.X);
                return cannonRotation;
            }
            set { cannonRotation = value; }
        }
        private float cannonRotation;

        public Weapon Cannon
        {
            get{ return cannon; }
        }
        private Weapon cannon;

        public override float TotalHealth
        {
            get { return MaxHealth; }
        }

        private float fireTime = 0;
        private float overheatTime;
        private bool isOverheated;

        private bool Flipped
        {
            get { return velocity.X < 0; }
        }

        private float moveSoundTime = 0;
        private float maxMoveSoundTime = 0.1f;

        // Animations
        private AnimationPlayer sprite;
        private Animation moveAnimation;

        private AnimationPlayer weaponSprite;
        private Animation weaponIdleAnimation;
        private Animation weaponFireAnimation;

        private Cue deathCue;
        public Cue DeathCue
        {
            get { return deathCue; }
        }

        #region Constants

        /// <summary>
        /// The maximum health of the heli.
        /// </summary>
        public float MaxHealth;

        /// <summary>
        /// The speed at which this enemy moves.
        /// </summary>
        private float MaxMoveSpeed;

        /// <summary>
        /// The maximum up-down speed of the heli.
        /// </summary>
        private float MaxVerticalSpeed;

        /// <summary>
        /// The time it takes the heli's weapon to overheat.
        /// </summary>
        private float MaxOverheatTime;

        /// <summary>
        /// The speed at which the heli accelerates (per second per second).
        /// </summary>
        private float Acceleration;

        /// <summary>
        /// How fast the gun will overheat.
        /// </summary>
        private float OverheatFactor;
        
        /// <summary>
        /// The distance the heli will try to remain from the player.
        /// </summary>
        private const float OptimumDistFromPlayer = 250f;

        /// <summary>
        /// The minimum elevation the heli must maintain.
        /// </summary>
        private const float MinimumElevation = 480;

        #endregion

        #endregion

        #region Loading

        public Heli(Level level, int ident)
            : base(level, ident)
        {
        }

        /// <summary>
        /// Spawns an enemy at the specified position, and with the specified sprite set and spawn point.
        /// </summary>
        public void InitEnemy(Vector2 position, EnemySpawnPoint point, HeliDataHolder data)
        {
            float waveFactor = 1 + (float)(level.Wave - ID * 3 - 6) / 10f;
            if (waveFactor > 1)
            {
                waveFactor = 1 + (float)(level.Wave - (level.Wave / 6) * 2 - ID * 3 - 6) / 10f;
                waveFactor = (float)Math.Pow(waveFactor, .5);
            }
            waveFactor = Math.Max(waveFactor, .5f);

            // Initialize constants.
            MaxHealth = data.MaxHealth * waveFactor;
            MaxMoveSpeed = data.MaxMoveSpeed;
            MaxVerticalSpeed = data.MaxVerticalSpeed;
            Acceleration = data.Acceleration;
            polygon = Polygon.MakePolygon(data.Polygon);
            MaxOverheatTime = data.MaxOverheatTime;
            OverheatFactor = data.OverheatFactor;

            // Load animations.
            moveAnimation = new Animation(data.MoveAnimation, 0.1f, true, 1.5f);

            sprite.OriginType = OriginType.Center;
            sprite.PlayAnimation(moveAnimation);

            // Load the heli's weapon
            WeaponData weapon = data.Weapon;

            cannon = new Weapon(weapon);
            cannon.BulletTexture = data.BulletTexture;
            cannon.FireTextParticle = data.FireTextParticle;
            cannon.Damage *= waveFactor;

            fireTime = 1f;

            weaponFireAnimation = new Animation(data.WeaponFire, Cannon.AnimationLength, false, 5.3333f);
            weaponIdleAnimation = new Animation(data.WeaponIdle, Cannon.AnimationLength, false, 5.3333f);

            weaponSprite.OriginType = OriginType.Center;
            weaponSprite.PlayAnimation(weaponIdleAnimation);

            cannonPosition = new Vector2(position.X, position.Y + sprite.Animation.FrameHeight * .25f);

            base.InitEnemy(position, point);

            velocity = Vector2.Zero;
            health = MaxHealth;
            polygon.Position = this.position;
            GetCannonPositon(0);
        }
        
        #endregion

        #region Update

        /// <summary>
        /// Does its best to hover at a certain distance above the player.
        /// </summary>
        public override void Update(float elapsed)
        {
            base.Update(elapsed);

            if (state == EnemyState.Alive)
            {
                sprite.Update(elapsed);
                weaponSprite.Update(elapsed);

                int Xdirection = 0;

                Vector2 distFromPlayer = Position - level.Player.Position;

                if (id == 2)
                {
                    if (isOverheated)
                    {
                        if (distFromPlayer.X < 0)
                            Xdirection = 1;
                        else
                            Xdirection = -1;
                    }
                    else
                    {
                        Xdirection = velocity.X > 0 ? 1 : -1;
                    }
                }
                else
                {
                    if (distFromPlayer.X < 0)
                        Xdirection = 1;
                    else
                        Xdirection = -1;

                    foreach (Enemy e in level.Enemies)
                    {
                        if (e.GetType() == typeof(Heli) && e != this)
                        {
                            Heli h = (Heli)e;

                            Point left = new Point((int)(this.CollisionPolygon.Center.X - this.CollisionPolygon.Radius),
                                (int)this.CollisionPolygon.Center.Y);
                            Point right = new Point((int)(this.CollisionPolygon.Center.X + this.CollisionPolygon.Radius),
                                (int)this.CollisionPolygon.Center.Y);

                            if (h.CollisionPolygon.BoundingBox.Contains(left))
                                Xdirection = 1;

                            if (h.CollisionPolygon.BoundingBox.Contains(right))
                                Xdirection = -1;
                        }
                    }
                }

                if (level.GetTilePosition(this.Position).X < 0)
                    Xdirection = 1;
                if (level.GetTilePosition(this.Position).X > level.Width)
                    Xdirection = -1;

                velocity.X += Xdirection * Acceleration * elapsed;

                if (distFromPlayer.Y < -OptimumDistFromPlayer - 25 &&
                    position.Y < level.LevelBounds.Height - MinimumElevation - 50)
                {
                    velocity.Y += Acceleration * elapsed;
                }
                else if (distFromPlayer.Y > -OptimumDistFromPlayer + 25)
                {
                    velocity.Y -= Acceleration * elapsed;
                }
                else
                {
                    if ((velocity.Y < Acceleration * elapsed && velocity.Y >= 0) ||
                        (velocity.Y > -Acceleration * elapsed && velocity.Y <= 0))
                        velocity.Y = 0;
                    if (velocity.Y > Acceleration * elapsed)
                        velocity.Y -= Acceleration * elapsed;
                    if (velocity.Y < -Acceleration * elapsed)
                        velocity.Y += Acceleration * elapsed;
                }

                velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
                velocity.Y = MathHelper.Clamp(velocity.Y, -MaxVerticalSpeed, MaxVerticalSpeed);
                position = position + velocity * elapsed;
                polygon.Position = Position;
                polygon.Orientation = Rotation;
                polygon.flipped = Flipped;

                GetCannonPositon(elapsed);

                if (Math.Abs(distFromPlayer.X) < 1280)
                    DoFireWeapon(elapsed);

                overheatTime = MathHelper.Max(0, overheatTime - elapsed);
                if (overheatTime <= 0)
                    isOverheated = false;

                moveSoundTime += elapsed;
                if (moveSoundTime >= maxMoveSoundTime)
                {
                    level.Screen.SFXManager.PlayCue("Heli Move");

                    moveSoundTime -= maxMoveSoundTime;
                    while (moveSoundTime >= elapsed)
                        moveSoundTime -= elapsed;
                }
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw facing the way the enemy is moving.
            SpriteEffects flip;

            Color drawColor = damageToTake > 0 ? Color.Red : Color.White;
            drawColor *= state == EnemyState.Spawning ? 0.6f : 1f;

            sprite.Color = drawColor;
            weaponSprite.Color = drawColor;

            flip = Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip, Rotation);

            if (CannonRotation < -MathHelper.PiOver2 || CannonRotation > MathHelper.PiOver2)
                flip = SpriteEffects.None;
            else
            {
                flip = SpriteEffects.FlipHorizontally;
                cannonRotation = cannonRotation > 0 ? cannonRotation - MathHelper.Pi : cannonRotation + MathHelper.Pi;
            }

            weaponSprite.Draw(gameTime, spriteBatch, CannonPosition, flip, cannonRotation);

            if (Vector2.Subtract(cannonPosition, position).Length() > 40f && cannon != null)
                return;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Called when the heli's gun is firing.
        /// </summary>
        private void DoFireWeapon(float elapsed)
        {
            fireTime += elapsed;

            if (fireTime >= cannon.FireRate && !isOverheated)
            {
                fireTime = 0;

                Vector2 bulletPos = cannonPosition + cannonDirection * 36;

                Bullet[] bulletsToAdd = cannon.Fire(bulletPos, cannonDirection, this.Level, false);

                if (bulletsToAdd != null)
                {
                    for (int i = 0; i < bulletsToAdd.GetLength(0); i++)
                        Level.EnemyBullets.Add(bulletsToAdd[i]);

                    overheatTime += cannon.FireRate * OverheatFactor;
                    if (overheatTime >= MaxOverheatTime)
                        isOverheated = true;
                }

                weaponSprite.PlayAnimation(weaponIdleAnimation);
                weaponSprite.PlayAnimation(weaponFireAnimation);
            }
        }

        /// <summary>
        /// Calculates the position of the gun's origin, relative to the heli's.
        /// </summary>
        /// <param name="elapsed"></param>
        private void GetCannonPositon(float elapsed)
        {
            Matrix mat = Matrix.CreateRotationZ(this.rotation) *
                Matrix.CreateTranslation(Position.X, Position.Y, 0);

            cannonPosition = Vector2.Transform(new Vector2(0, sprite.Animation.FrameHeight * .25f), mat);

            Vector2 playerPosition = Level.Player.Position;
            playerPosition.Y -= 50;

            directionToPlayer = GetDirectionToFire();

            if (elapsed > 0)
                cannonDirection = One_Man_Army_Game.StepSlowlyToVector(elapsed, 600,
                        directionToPlayer, cannonDirection);
            else
                cannonDirection = directionToPlayer;
        }

        /// <summary>
        /// The "leading" algorithm that calculates what direction to fire in.
        /// </summary>
        private Vector2 GetDirectionToFire()
        {
            if (cannon.MovementType == BulletMovementType.ParabolicArc)
            {
                return Vector2.Normalize(new Vector2(velocity.X > 0 ? 1 : -1, 0));
            }

            Vector2 playerPosition = Level.Player.Position;
            playerPosition.Y -= 50;

            Vector2 distanceToPlayer = playerPosition - cannonPosition;
            distanceToPlayer -= Vector2.Normalize(distanceToPlayer) * 36;

            Vector2 directionToFire;

            float C = (float)Math.Atan2(distanceToPlayer.X, distanceToPlayer.Y);

            float a = distanceToPlayer.Length();
            float cosC = (float)Math.Cos(C);
            float b = Math.Abs(Level.Player.LastFrameVelocity.X * 1.5f);
            float c = Cannon.Velocity;

            float a2 = -2 * a * b * cosC;
            float c2 = 2 * Math.Abs(c * c - b * b);
            float b2 = (float)Math.Sqrt(a2 * a2 + 2 * c2 * a * a);

            float t1 = (a2 + b2) / c2;
            float t2 = (a2 - b2) / c2;

            float t = Math.Max(t1, t2);

            Vector2 futurePositionOfPlayer = new Vector2(playerPosition.X +
                Level.Player.LastFrameVelocity.X * t, playerPosition.Y);

            directionToFire = futurePositionOfPlayer - cannonPosition;

            return Vector2.Normalize(directionToFire);
        }

        #endregion
    }
}
