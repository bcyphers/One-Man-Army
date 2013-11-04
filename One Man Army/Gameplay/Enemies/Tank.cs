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

    #region TankDataHolder

    public struct TankDataHolder
    {
        public TankDataHolder(TankData data)
        {
            MaxHealth = data.MaxHealth;
            ShotsPerPause = data.ShotsPerPause;
            MoveSpeed = data.MoveSpeed;
            OptimumDistFromPlayer = data.OptimumDistFromPlayer;
            Polygon = data.Polygon;
            MaxOverheatTime = data.MaxOverheatTime;
            OverheatFactor = data.OverheatFactor;

            RunAnimation = null;
            IdleAnimation = null;
            WeaponFire = null;
            WeaponIdle = null;
            BulletTexture = null;
            FireTextParticle = null;

            Weapon = null;
        }

        public float MaxHealth;
        public int ShotsPerPause;
        public float MoveSpeed;
        public float OptimumDistFromPlayer;
        public List<Vector2> Polygon;
        public float MaxOverheatTime;
        public float OverheatFactor;

        public Texture2D RunAnimation;
        public Texture2D IdleAnimation;
        public Texture2D WeaponFire;
        public Texture2D WeaponIdle;
        public Texture2D BulletTexture;
        public Texture2D FireTextParticle;

        public WeaponData Weapon;
    }

    #endregion

    /// <summary>
    /// A tank which rolls back and forth along the ground.
    /// </summary>
    public class Tank : Enemy
    {
        #region Fields

        public bool IsFiring
        {
            get { return isFiring; }
        }
        private bool isFiring = false;

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

        public Weapon Cannon;

        private float fireTime = 0;
        private float overheatTime;
        private bool isOverheated;

        public override Vector2 Center
        {
            get { return new Vector2(position.X, position.Y - localBounds.Height / 2); }
        }

        public override float TotalHealth
        {
            get { return MaxHealth; }
        }

        // Animations
        private AnimationPlayer sprite;
        private Animation runAnimation;
        private Animation idleAnimation;

        private AnimationPlayer weaponSprite;
        private Animation weaponIdleAnimation;
        private Animation weaponFireAnimation;
        
        private Cue moveCue;
        public Cue MoveCue
        {
            get { return moveCue; }
        }

        private Cue deathCue;
        public Cue DeathCue
        {
            get { return deathCue; }
        }

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        private float waitTime;

        #region Constants

        /// <summary>
        /// The maximum health of the tank.
        /// </summary>
        public float MaxHealth;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        private float MaxWaitTime;

        /// <summary>
        /// How many shots to fire while paused: determines wait time.
        /// </summary>
        private int ShotsPerPause;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        private float MoveSpeed;

        /// <summary>
        /// The distance the tank will try to remain from the player.
        /// </summary>
        private float OptimumDistFromPlayer;

        /// <summary>
        /// The time it takes the tank's weapon to overheat.
        /// </summary>
        private float MaxOverheatTime;

        /// <summary>
        /// How fast the gun will overheat.
        /// </summary>
        private float OverheatFactor;

        #endregion

        protected Rectangle localBounds;

        /// <summary>
        /// Gets a rectangle which bounds this tank in world space, used for collisions with the player.
        /// </summary>
        public Rectangle InnerBounds
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        /// <summary>
        /// Gets a rectangle with the outer bounds of this tank, used for AI logic.
        /// </summary>
        public Rectangle OuterBounds
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X);
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y);

                return new Rectangle(left, top, sprite.Animation.FrameWidth, sprite.Animation.FrameHeight);
            }
        }

        private bool isOffRightEdge = false;
        private bool isOffLeftEdge = false;
        private bool isOnGround = true;
        public bool IsOnGround
        {
            get { return isOnGround; }
        }

        Vector2 velocity;
        public Vector2 Velocity
        {
            get { return velocity; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Tank(Level level, int ident)
            : base(level, ident)
        {
        }

        /// <summary>
        /// Spawns an enemy at the specified position, and with the specified sprite set and spawn point.
        /// </summary>
        public void InitEnemy(Vector2 position, EnemySpawnPoint point, TankDataHolder data)
        {
            float waveFactor = (float)(level.Wave - ID * 3 - 6) / 10f;
            if (waveFactor > 0)
                waveFactor *= .5f;
            waveFactor += 1;
            waveFactor = Math.Max(waveFactor, .5f);

            // Initialize constants.
            MaxHealth = data.MaxHealth * waveFactor;
            MoveSpeed = data.MoveSpeed;
            ShotsPerPause = data.ShotsPerPause;
            OptimumDistFromPlayer = data.OptimumDistFromPlayer;
            polygon = Polygon.MakePolygon(data.Polygon);
            MaxOverheatTime = data.MaxOverheatTime;
            OverheatFactor = data.OverheatFactor; 
            
            // Load base animations
            runAnimation = new Animation(data.RunAnimation, 0.2f, true, 1.6f);
            idleAnimation = new Animation(data.IdleAnimation, 0.15f, true, 1.6f);

            sprite.PlayAnimation(idleAnimation);

            // Load sounds
            moveCue = level.Game.SFXBank.GetCue("Tank Move");

            // Load the tank's weapon
            WeaponData weapon = data.Weapon;

            // Load weapon textures
            Cannon = new Weapon(weapon);
            Cannon.BulletTexture = data.BulletTexture;
            Cannon.FireTextParticle = data.FireTextParticle;
            Cannon.Damage *= waveFactor;

            weaponFireAnimation = new Animation(data.WeaponFire, Cannon.AnimationLength, false, 5.3333f);
            weaponIdleAnimation = new Animation(data.WeaponIdle, Cannon.AnimationLength, false, 5.3333f);

            weaponSprite.OriginType = OriginType.Center;
            weaponSprite.PlayAnimation(weaponIdleAnimation);

            MaxWaitTime = ShotsPerPause * Cannon.FireRate;

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 1);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameHeight * 0.65f);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            base.InitEnemy(position, point);

            health = MaxHealth;
            polygon.Position = this.position;
            cannonDirection = Vector2.Normalize(Level.Player.Position - this.Position);
            this.cannonPosition = new Vector2(this.position.X, this.position.Y -
                this.sprite.Animation.FrameHeight * 13 / 16);
            isOnGround = true;
            fireTime = Cannon.FireRate - Cannon.FireRate / 3;
        }

        #endregion

        #region Update & Draw

        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public override void Update(float elapsed)
        {
            base.Update(elapsed);

            if (state == EnemyState.Alive)
            {
                sprite.Update(elapsed);
                weaponSprite.Update(elapsed);

                int HasToFace;

                // Calculate tile position based on the side we are walking towards.
                velocity.X = 0;

                float distFromPlayer = this.Position.X - level.Player.Position.X;

                if (isOnGround)
                {
                    HasToFace = CheckForCollisions(elapsed);

                    if (waitTime > 0 && HasToFace == 0)
                    {
                        if (Math.Abs(distFromPlayer) <= 1280)
                            DoFireWeapon(elapsed);
                        else
                            waitTime += elapsed;

                        // Wait for some amount of time.
                        waitTime = Math.Max(0.0f, waitTime - elapsed);
                        if (waitTime <= 0.0f)
                        {
                            if (distFromPlayer < OptimumDistFromPlayer && distFromPlayer >= 0)
                                direction = (FaceDirection)(1);
                            if (distFromPlayer < -OptimumDistFromPlayer)
                                direction = (FaceDirection)(1);
                            if (distFromPlayer > -OptimumDistFromPlayer && distFromPlayer < 0)
                                direction = (FaceDirection)(-1);
                            if (distFromPlayer > OptimumDistFromPlayer)
                                direction = (FaceDirection)(-1);
                        }
                    }
                    else if (HasToFace != 0)
                    {
                        direction = (FaceDirection)HasToFace;
                        waitTime = 0;
                    }
                    else
                    {
                        // If we are the optimum distance from the player
                        if (Math.Abs(this.Position.X - level.Player.Position.X) <= OptimumDistFromPlayer + 5 &&
                            Math.Abs(this.Position.X - level.Player.Position.X) > OptimumDistFromPlayer - 5)
                        {
                            fireTime = Cannon.FireRate - Cannon.FireRate / 3;
                            waitTime = MaxWaitTime;
                        }
                        else
                        {
                            if (distFromPlayer < OptimumDistFromPlayer && distFromPlayer >= 0)
                                direction = (FaceDirection)(1);
                            if (distFromPlayer < -OptimumDistFromPlayer)
                                direction = (FaceDirection)(1);
                            if (distFromPlayer > -OptimumDistFromPlayer && distFromPlayer < 0)
                                direction = (FaceDirection)(-1);
                            if (distFromPlayer > OptimumDistFromPlayer)
                                direction = (FaceDirection)(-1);

                            if (direction == FaceDirection.Right)
                            {
                                if (!isOffRightEdge)
                                    velocity = new Vector2((int)direction * MoveSpeed, 0.0f);
                            }
                            if (direction == FaceDirection.Left)
                            {
                                if (!isOffLeftEdge)
                                    velocity = new Vector2((int)direction * MoveSpeed, 0.0f);
                            }
                        }
                    }
                }

                if (!isOnGround)
                {
                    sprite.PlayAnimation(idleAnimation);

                    float GravityAcceleration = 500.0f;
                    float MaxFallSpeed = 1000.0f;

                    velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
                    position.Y += velocity.Y * elapsed;

                    // Get the tank's bounding rectangle and find neighboring tiles.
                    Rectangle bounds = OuterBounds;
                    int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
                    int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
                    int tileY = (int)Level.GetTilePosition(position).Y;

                    for (int x = leftTile; x < rightTile + 1; x++)
                    {
                        if (level.GetCollision(x, tileY) == TileCollision.Impassable ||
                            level.GetCollision(x, tileY) == TileCollision.Platform)
                        {
                            isOnGround = true;
                            position.Y = tileY * Tile.Height;
                            velocity.Y = 0;
                        }
                    }
                }

                position.X += velocity.X * elapsed;
                position = new Vector2((float)Math.Round(position.X), (float)Math.Round(position.Y));

                directionToPlayer = Vector2.Normalize(Level.Player.ArmPosition - this.CannonPosition);

                if (fireTime > 0.5f)
                    cannonDirection = One_Man_Army_Game.StepSlowlyToVector(elapsed, 400,
                        directionToPlayer, cannonDirection);

                this.polygon.Position = this.position;

                this.cannonPosition = new Vector2(this.position.X, this.position.Y -
                    this.sprite.Animation.FrameHeight * 13 / 16);
            }

            overheatTime = MathHelper.Max(0, overheatTime - elapsed);
            if (overheatTime <= 0)
                isOverheated = false;
        }

        /// <summary>
        /// Checks for, and resolves, collisions with neighboring tiles. Returns whether the 
        /// sprite's direction has been decided yet.
        /// </summary>
        /// <param name="elapsed"></param>
        private int CheckForCollisions(float elapsed)
        {
            bool wasOffRightEdge = isOffRightEdge;
            bool wasOffLeftEdge = isOffLeftEdge;

            isOffRightEdge = false;
            isOffLeftEdge = false;

            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            int leftSideBuffer = wasOffLeftEdge && direction == FaceDirection.Right ? 0 : 5;
            int rightSideBuffer = wasOffRightEdge && direction == FaceDirection.Left ? 0 : 5;

            Vector2 leftBottom = new Vector2(
                this.Position.X - localBounds.Width / 2 + leftSideBuffer, this.Position.Y + 1);
            Vector2 rightBottom = new Vector2(
                this.Position.X + localBounds.Width / 2 - rightSideBuffer, this.Position.Y + 1);
            Vector2 leftSide = new Vector2(
                this.Position.X - localBounds.Width / 2 + leftSideBuffer, this.Position.Y - 1);
            Vector2 rightSide = new Vector2(
                this.Position.X + localBounds.Width / 2 - rightSideBuffer, this.Position.Y - 1);
            Vector2 frontSide = new Vector2(
                this.Position.X + (localBounds.Width * (int)direction) / 2 + (int)direction, this.Position.Y - 1);

            TileCollision leftBottomCollision = level.GetCollision(leftBottom);
            TileCollision rightBottomCollision = level.GetCollision(rightBottom);
            TileCollision leftSideCollision = level.GetCollision(leftSide);
            TileCollision rightSideCollision = level.GetCollision(rightSide);

            foreach (Enemy e in level.Enemies)
            {
                if (e.GetType() == typeof(Tank) && e != this)
                {
                    Tank t = (Tank)e;

                    if (t.InnerBounds.Contains((int)leftSide.X, (int)leftSide.Y))
                        isOffLeftEdge = true;

                    if (t.InnerBounds.Contains((int)rightSide.X, (int)rightSide.Y))
                        isOffRightEdge = true;
                }
            }

            if ((leftBottomCollision != TileCollision.Platform &&
                leftBottomCollision != TileCollision.Impassable) ||
                leftSideCollision != TileCollision.Passable)
                isOffLeftEdge = true;

            if ((rightBottomCollision != TileCollision.Platform &&
                rightBottomCollision != TileCollision.Impassable) ||
                rightSideCollision != TileCollision.Passable)
                isOffRightEdge = true;

            if (isOffRightEdge && isOffLeftEdge)
            {
                if (level.GetCollision(new Vector2(this.Position.X, this.Position.Y + 1)) ==
                    TileCollision.Platform ||
                    level.GetCollision(new Vector2(this.Position.X, this.Position.Y + 1)) ==
                    TileCollision.Impassable)
                {
                    isOnGround = true;
                    if (waitTime <= 0)
                    {
                        waitTime = MaxWaitTime;
                    }
                }
                else
                    isOnGround = false;

                return 0;
            }

            if (isOffLeftEdge && !isOffRightEdge)
            {
                velocity = new Vector2((int)direction * MoveSpeed, 0.0f);
                return 1;
            }

            if (isOffRightEdge && !isOffLeftEdge)
            {
                velocity = new Vector2((int)direction * MoveSpeed, 0.0f);
                return -1;
            }

            // If we are about to run into a wall or off a cliff, start waiting.
            if ((Level.GetCollision(tileX + (int)direction, tileY - 1) != TileCollision.Passable ||
                (Level.GetCollision(tileX + (int)direction, tileY) != TileCollision.Platform &&
                Level.GetCollision(tileX + (int)direction, tileY) != TileCollision.Impassable)) &&
                waitTime <= 0)
            {
                fireTime = Cannon.FireRate - Cannon.FireRate / 3;
                waitTime = MaxWaitTime;

                if (direction == FaceDirection.Right)
                    isOffRightEdge = true;
                else
                    isOffLeftEdge = true;

                return 0;
            }

            // If we are about to run into another tank, start waiting.
            foreach (Enemy e in level.Enemies)
            {
                if (e.GetType() == typeof(Tank) && e != this)
                {
                    Tank t = (Tank)e;

                    if (t.OuterBounds.Contains((int)frontSide.X, (int)frontSide.Y) &&
                        waitTime <= 0)
                    {
                        fireTime = Cannon.FireRate - Cannon.FireRate / 3;
                        waitTime = MaxWaitTime;

                        if (direction == FaceDirection.Right)
                            isOffRightEdge = true;
                        else
                            isOffLeftEdge = true;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Stop running when the game is paused or before turning around.
            if (!Level.Player.IsAlive || waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
                if (moveCue.IsPlaying)
                    moveCue.Pause();
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
                if (moveCue.IsPaused)
                        moveCue.Resume();
                if (!moveCue.IsPlaying && moveCue.IsPrepared)
                        moveCue.Play();
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip;

            Color drawColor = damageToTake > 0 ? Color.Red : Color.White;
            drawColor *= state == EnemyState.Spawning ? 0.6f : 1f;

            sprite.Color = drawColor;
            weaponSprite.Color = drawColor;

            flip = direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            sprite.Draw(gameTime, spriteBatch, Position, flip, 0f);

            if (CannonRotation < -MathHelper.PiOver2 || CannonRotation > MathHelper.PiOver2)
                flip = SpriteEffects.None;
            else
            {
                flip = SpriteEffects.FlipHorizontally;
                cannonRotation = cannonRotation > 0 ? cannonRotation - MathHelper.Pi : cannonRotation + MathHelper.Pi;
            }
            weaponSprite.Draw(gameTime, spriteBatch, CannonPosition, flip, cannonRotation);
        }

        /// <summary>
        /// Called when the tank's cannon is firing.
        /// </summary>
        private void DoFireWeapon(float elapsed)
        {
            fireTime += elapsed;

            if (fireTime >= Cannon.FireRate && !isOverheated)
            {
                if (cannonDirection == directionToPlayer)
                {
                    fireTime = 0;

                    Vector2 bulletPos = cannonPosition + cannonDirection * 44;

                    Bullet[] bulletsToAdd = Cannon.Fire(bulletPos, cannonDirection, this.Level, false);

                    if (bulletsToAdd != null)
                    {
                        for (int i = 0; i < bulletsToAdd.GetLength(0); i++)
                            Level.EnemyBullets.Add(bulletsToAdd[i]);

                        overheatTime += Cannon.FireRate * OverheatFactor;
                        if (overheatTime >= MaxOverheatTime)
                            isOverheated = true;
                    }

                    weaponSprite.PlayAnimation(weaponIdleAnimation);
                    weaponSprite.PlayAnimation(weaponFireAnimation);

                    //smoke.AddParticles(bulletPos, Cannon.BulletRadius * 2);
                }
                else
                {
                    waitTime += elapsed;
                }
            }
        }

        #endregion
    }
}
