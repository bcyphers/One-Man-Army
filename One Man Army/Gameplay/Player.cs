using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace One_Man_Army
{
    /// <summary>
    /// Our indestructible badass
    /// </summary>
    public class Player
    {
        #region Fields

        public PlayerIndex ControllingPlayer
        {
            get { return controllingPlayer; }
            set { controllingPlayer = value; }
        }
        private PlayerIndex controllingPlayer;

        private bool isFlagSet = false;

        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation dieAnimation;
        private Animation armsAnimation;
        private Animation fireAnimation;
        private Animation finishAnimation;
        private Animation rageAnimation;
        private Animation crouchRunAnimation;
        private Animation crouchIdleAnimation;
        private AnimationPlayer sprite;
        private AnimationPlayer weaponSprite;
        private AnimationPlayer overlayAnimation;

        private Texture2D pressY;
        public Cue RageModeCue;

        public Vector2 LastFrameVelocity;

        /// <summary>
        /// Level the player belongs to.
        /// </summary>
        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Is the player alive?
        /// </summary>
        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        /// <summary>
        /// Is the player crouching?
        /// </summary>
        public bool IsCrouching
        {
            get { return isCrouching; }
        }
        bool isCrouching;

        /// <summary>
        /// The most health the player can have at any moment; what he will have when 
        /// he gets a health pack
        /// </summary>
        public float MaxHealth
        {
            get { return maxHealth; }
        }
        private float maxHealth;

        /// <summary>
        /// The player's health.
        /// </summary>
        public float Health
        {
            get { return health; }
            set { health = value; }
        }
        float health;

        private const float MaxHealthRegenTime = 3f;
        private float healthRegenTime = 0f;

        public float CurrentMaxHealth
        {
            get { return currentMaxHealth; }
        }
        private float currentMaxHealth;

        /// <summary>
        /// The player's rage meter: Once it reaches 1, the player can activate Rage Mode.
        /// </summary>
        public float Rage
        {
            get { return rage; }
        }
        private float rage = 0f;

        /// <summary>
        /// Is Rage Mode currently active?
        /// </summary>
        public bool IsRageMode
        {
            get { return isRageMode; }
        }
        private bool isRageMode;

        private float rageDegradeGraceTime;
        const float MaxRageDegradeGraceTime = 1f;
        const float RageDegradeSpeed = 0.15f;
        const float RageMultiplyer = 1f;
        private bool isRageBarFull;

        // Finishing move
        private bool shouldFinish;
        private bool isFinishing;
        private Tank tankFinishing;
        private float finishTime = 0f;
        const float MaxFinishingTime = 1f;

        // Physics state
        public Vector2 Position
        {
            get
            {
                return position;
                //return new Vector2((float)Math.Round(position.X), (float)Math.Round(position.Y)); 
            }
            set { position = value; }
        }
        Vector2 position;

        /// <summary>
        /// The position to draw the arms at.
        /// </summary>
        public Vector2 ArmPosition
        {
            get
            {
                return IsCrouching ?
                    new Vector2(Position.X, Position.Y - sprite.Animation.FrameHeight * 2 / 3) :
                    new Vector2(Position.X, Position.Y - sprite.Animation.FrameHeight * 2 / 3);
            }
        }

        /// <summary>
        /// All the player's weapons.
        /// </summary>
        public List<Weapon> Weapons
        {
            get { return weapons; }
            set { weapons = value; }
        }
        private List<Weapon> weapons = new List<Weapon>();

        /// <summary>
        /// The player's current weapon.
        /// </summary>
        public Weapon CurrentWeapon
        {
            get { return weapons[currentWeapon]; }
        }

        /// <summary>
        /// Is the player currently firing his weapon?
        /// </summary>
        public bool IsFiring
        {
            get { return isFiring; }
        }
        private bool isFiring = false;


        public int currentWeapon = 0;
        private float fireTime;

        private float previousBottom;

        /// <summary>
        /// The player's velocity.
        /// </summary>
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        /// <summary>
        /// The direction the player is aiming.
        /// </summary>
        public Vector2 ArmDirection
        {
            get { return armDirection; }
            set { armDirection = value; }
        }
        Vector2 armDirection = Vector2.UnitX;

        /// <summary>
        /// The rotation of the player's arms.
        /// </summary>
        public float ArmRotation
        {
            get
            {
                armRotation = (float)Math.Atan2(armDirection.Y - 0, armDirection.X - 0);
                return armRotation;
            }
            set { armRotation = value; }
        }
        private float armRotation;


        // Constants for controling horizontal movement
        private const float MoveAcceleration = 14000.0f;
        private const float MaxMoveSpeed = 2000.0f;
        private const float AirDragFactor = 20f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -1000.0f;
        private const float GravityAcceleration = 5000.0f;
        private const float MaxFallSpeed = 1000.0f;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        bool isOnRamp = false;
        Tank standingOnTank = null;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        /// <summary>
        /// Gets a rectangle which represents the collidable portion of the player sprite
        /// </summary>
        private Rectangle localBounds
        {
            get
            {
                // Calculate bounds within texture size.
                int width, left, height, top;

                if (IsCrouching)
                {
                    width = (int)(crouchIdleAnimation.FrameWidth * 0.5);
                    left = (crouchIdleAnimation.FrameWidth - width) / 2;
                    height = 48; //(int)(crouchIdleAnimation.FrameHeight * 0.9);
                    top = crouchIdleAnimation.FrameHeight - height;
                }
                else
                {
                    width = (int)(idleAnimation.FrameWidth * 0.5);
                    left = (idleAnimation.FrameWidth - width) / 2;
                    height = (int)(idleAnimation.FrameHeight * 0.9);
                    top = idleAnimation.FrameHeight - height;
                }

                return new Rectangle(left, top, width, height);
            }
        }

        /// <summary>
        /// Gets a rectangle which bounds this player in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        public Polygon CollisionPolygon
        {
            get { return polygon; }
        }
        private Polygon polygon;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructors a new player.
        /// </summary>
        public Player(Level level, Vector2 position, List<Weapon> startWeapons)
        {
            this.level = level;

            foreach (Weapon w in startWeapons)
            {
                this.weapons.Add(w);
                if (w.State != WeaponState.HeldByPlayer)
                    w.Activate();
            }

            LoadContent();

            Reset(position);
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true, 0.8f);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true, 0.8f);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, false, 0.8f);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, false, 0.8f);
            finishAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Finish Tank"), 0.09f, false, 1f);
            crouchIdleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Crouch Idle"), 0.1f, false, 4f / 3f);
            crouchRunAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Crouch Run"), 0.1f, true, 4f / 3f);
            //rageAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Rage"), 0.1f, false, 0.64f);

            RageModeCue = level.Game.SFXBank.GetCue("Heartbeat");

#if XBOX
            pressY = level.Content.Load<Texture2D>("HUD/Press Y");
#else
            pressY = level.Content.Load<Texture2D>("HUD/Press F");
#endif

            armsAnimation = new Animation(weapons[currentWeapon].ArmsTexture,
                weapons[currentWeapon].AnimationLength, false, 2f);
            fireAnimation = new Animation(weapons[currentWeapon].FireAnimation,
                weapons[currentWeapon].AnimationLength, false, 2f);

            sprite.OriginType = OriginType.BottomMiddle;
            weaponSprite.OriginType = OriginType.Center;
            overlayAnimation.OriginType = OriginType.BottomMiddle;

            weaponSprite.PlayAnimation(armsAnimation);
            //overlayAnimation.PlayAnimation(rageAnimation);

            // Create the collision polygon.
            polygon = Polygon.MakeRectanglePolygon(localBounds);
            for (int i = 0; i < 4; i++)
            {
                polygon.relativeVertices[i] = new Vector2(polygon.relativeVertices[i].X,
                    polygon.relativeVertices[i].Y - localBounds.Height / 2);
            }
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 pos)
        {
            position = pos;
            Velocity = Vector2.Zero;
            isAlive = true;
            isFiring = false;
            sprite.PlayAnimation(idleAnimation);
            weaponSprite.PlayAnimation(fireAnimation);
            maxHealth = 1f + (level.Wave / 3) * .25f;
            health = maxHealth;
            currentMaxHealth = maxHealth;
        }

        #endregion

        #region Update & Draw

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        public void Update(float elapsed)
        {
            if (isRageMode)
            {
                if (RageModeCue.IsPaused)
                    RageModeCue.Resume();
                elapsed *= 2f;
            }

            sprite.Update(elapsed);
            weaponSprite.Update(elapsed);
            // overlayAnimation.Update(elapsed);

            if (!isFinishing)
            {
                ApplyPhysics(elapsed);

                if (IsAlive && !isJumping)
                {
                    if (IsCrouching)
                    {
                        if (movement != 0)
                            sprite.PlayAnimation(crouchRunAnimation);
                        else
                            sprite.PlayAnimation(crouchIdleAnimation);
                    }
                    else
                    {
                        if (movement != 0)
                            sprite.PlayAnimation(runAnimation);
                        else
                            sprite.PlayAnimation(idleAnimation);
                    }
                }

                DoFireWeapon(elapsed);

                if (weapons[currentWeapon].State == WeaponState.Inactive)
                {
                    weapons.RemoveAt(currentWeapon);
                    if (currentWeapon >= weapons.Count)
                        currentWeapon = 0;

                    SwitchWeapon();
                }

                foreach (Enemy e in Level.Enemies)
                {
                    if (shouldFinish && e is Tank && e.State == EnemyState.Alive &&
                        (e.Health <= e.TotalHealth * 0.5 || e.Health <= 0.25f) &&
                        Math.Abs(e.Center.Y - this.Position.Y) < 48 && Math.Abs(e.Center.X - this.Position.X) < 128)
                    {
                        isFinishing = true;
                        tankFinishing = e as Tank;
                        finishTime = MaxFinishingTime;
                        sprite.PlayAnimation(finishAnimation);
                        break;
                    }
                }
            }
            else
            {
                isFinishing = tankFinishing.State == EnemyState.Alive ? true : false;

                Vector2 distanceToTank = Vector2.Subtract(
                    new Vector2(tankFinishing.Center.X +
                        (this.Position.X > tankFinishing.Position.X ? 32 : -32),
                        tankFinishing.InnerBounds.Top),
                    this.Position);

                if (distanceToTank.Length() >= 0.00001f)
                    position += Vector2.Normalize(distanceToTank) *
                        MathHelper.Min(MaxMoveSpeed * elapsed * 0.5f, distanceToTank.Length());

                finishTime -= elapsed;
                if (finishTime <= 0)
                    tankFinishing.TakeDamage(tankFinishing.Health + 0.1f);

                polygon.Position = position;
            }

            if (isAlive)
            {
                healthRegenTime += elapsed;
                if (healthRegenTime >= MaxHealthRegenTime)
                {
                    health += 0.01f;
                    health = MathHelper.Min(health, currentMaxHealth);
                }
            }

            if (!isRageBarFull && rageDegradeGraceTime > MaxRageDegradeGraceTime)
                rage -= elapsed * RageDegradeSpeed * (isRageMode ? 0.35f : 1f);
            else
                rageDegradeGraceTime += elapsed;

            if (rage <= 0)
            {
                rage = 0;
                isRageMode = false;

                if (RageModeCue.IsPlaying)
                    RageModeCue.Pause();
                level.PublicMusicVolume = 1f;
            }

            // Clear input.
            movement = 0.0f;
            isJumping = false;
            isFiring = false;
        }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        public void HandleInput(InputState input)
        {
            movement = input.GetPlayerMovement(controllingPlayer);

            isJumping = (isOnGround && input.DidPlayerJump(controllingPlayer) ||
                !isOnGround && input.IsPlayerJumping(controllingPlayer));

            shouldFinish = (!isFinishing && input.IsPlayerFinishingMove(controllingPlayer));

            //if (input.IsToggleCrouch(controllingPlayer))
            //    isCrouching = !isCrouching;
            //if (isJumping)
            //    isCrouching = false;

            int lastWeapon = currentWeapon;

            currentWeapon += input.IsSwitchWeapon(controllingPlayer);

            if (currentWeapon >= weapons.Count)
                currentWeapon = 0;
            if (currentWeapon < 0)
                currentWeapon = weapons.Count - 1;

            if (lastWeapon != currentWeapon)
                SwitchWeapon();

#if !XBOX
            ArmDirection = Vector2.Normalize(new Vector2(input.CurrentMouseState.X + level.CameraPosition - ArmPosition.X,
                 input.CurrentMouseState.Y - ArmPosition.Y));
#endif
            Vector2? dir = input.GetPlayerAimDirection(controllingPlayer);
            if (dir.HasValue)
                ArmDirection = One_Man_Army_Game.StepSlowlyToVector(1f, 125, dir.Value, armDirection);

            isFiring = input.IsPlayerFiring(controllingPlayer);

            if (isRageBarFull && input.IsActivateRageMode(controllingPlayer))
            {
                isRageMode = true;
                isRageBarFull = false;

                if (RageModeCue.IsPaused)
                    RageModeCue.Resume();
                if (!RageModeCue.IsPlaying && RageModeCue.IsPrepared)
                    RageModeCue.Play();

                level.PublicMusicVolume = 0.5f;
            }

            if (!level.PlayerHasControl)
            {
                isFiring = false;
                movement = 0;
                isJumping = false;
                shouldFinish = false;
                return;
            }
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteEffects flip;

            if (isFlagSet)
            {
                sprite.Color = Color.Red;
                weaponSprite.Color = Color.Red;
            }
            else
            {
                sprite.Color = Color.White;
                weaponSprite.Color = Color.White;
            }

            isFlagSet = false;

            // Draw that sprite.
            if (isFinishing)
            {
                flip = this.Position.X > tankFinishing.Position.X ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                sprite.Draw(gameTime, spriteBatch, Position, flip, 0f);
            }
            else
            {
                Vector2 pressYPos;
                Vector2 pressYCenter = new Vector2(pressY.Width / 2f, pressY.Height / 2f);

                if (CanFinish(out pressYPos))
                    spriteBatch.Draw(pressY, pressYPos,
                        null, Color.White, 0f, pressYCenter, 0.8f, SpriteEffects.None, 0);

                // Flip the sprite to face the way we are moving.
                if (ArmRotation < -MathHelper.PiOver2 || ArmRotation > MathHelper.PiOver2)
                {
                    flip = SpriteEffects.FlipHorizontally;
                    ArmRotation += MathHelper.Pi;
                }
                else
                    flip = SpriteEffects.None;

                sprite.Draw(gameTime, spriteBatch, Position, flip, 0f);
                weaponSprite.Draw(gameTime, spriteBatch, ArmPosition, flip, armRotation);
            }
        }

        #endregion

        #region Physics & Collisions

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(float elapsed)
        {
            Vector2 previousPosition = Position;

            // Calculate Y velocity and position.
            velocity.Y += GravityAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, elapsed);

            position.Y += velocity.Y * elapsed;

            // Calculate X velocity and position.
            velocity.X += movement * MoveAcceleration * elapsed;

            // Apply pseudo-drag horizontally.
            velocity.X -= AirDragFactor * velocity.X * elapsed;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            position.X += velocity.X * elapsed;

            // If the player is standing on a tank, move the player with the tank.
            if (standingOnTank != null)
                position.X += standingOnTank.Velocity.X * elapsed;

            polygon.Position = position;

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, or the velocity is too low to be significant,
            // reset the velocity to zero.
            if (Math.Abs(velocity.X) < 5 ||
                (Math.Abs(velocity.X * elapsed) > 1 && (int)position.X == (int)previousPosition.X))
                velocity.X = 0;

            polygon.Position = position;

            LastFrameVelocity = (position - previousPosition) * (1 / elapsed);
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling((float)bounds.Right / Tile.Width) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling((float)(bounds.Bottom) / Tile.Height) - 1;

            // Was the player on a ramp last frame?
            bool wasOnRamp = isOnRamp;
            // Was the player on the ground last frame?
            bool wasOnGround = isOnGround;

            // Reset flag to search for ground collision.
            isOnGround = false;
            isOnRamp = false;
            standingOnTank = null;

            // A list of the edges of the tile to check for collisions with.
            List<TileEdge> collidableEdges = new List<TileEdge>();

            foreach (Enemy e in level.Enemies)
            {
                if (e is Tank)
                {
                    Tank t = e as Tank;

                    collidableEdges.Add(TileEdge.Left);
                    collidableEdges.Add(TileEdge.Right);
                    collidableEdges.Add(TileEdge.Top);
                    collidableEdges.Add(TileEdge.Bottom);

                    Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, t.InnerBounds);

                    if (depth != Vector2.Zero)
                    {
                        float absDepthX = Math.Abs(depth.X);
                        float absDepthY = Math.Abs(depth.Y);

                        if (absDepthY < absDepthX)
                        {
                            // If we crossed the top of a tank, we are on the ground.
                            if (previousBottom <= t.InnerBounds.Top)
                            {
                                standingOnTank = t;
                                isOnGround = true;
                                velocity.Y = 0;
                                position.Y = (int)Math.Round(position.Y);
                            }

                            // Kill the player if it is hit by the falling tank
                            if (!t.IsOnGround && wasOnGround && depth.Y > 0)
                                OnKilled(t);

                            // Resolve the collision along the Y axis.
                            position = new Vector2(position.X, position.Y + depth.Y);

                            // Perform further collisions with the new bounds.
                            bounds = BoundingRectangle;
                        }
                        else
                        {
                            // Resolve the collision along the X axis.
                            position = new Vector2(position.X + depth.X, position.Y);

                            // Perform further collisions with the new bounds.
                            bounds = BoundingRectangle;
                        }
                    }
                }
            }

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetTileBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);

                        float absDepthX = Math.Abs(depth.X);
                        float absDepthY = Math.Abs(depth.Y);

                        collidableEdges = Level.GetCollidableEdges(x, y);

                        // If the player is colliding with a ramp, and are not jumping
                        if ((collision == TileCollision.SlantedUp || collision == TileCollision.SlantedDown) &&
                            (x * Tile.Width <= position.X && position.X <= (x + 1) * Tile.Width) && velocity.Y >= 0)
                        {
                            // If the player is actually on the ramp
                            if (Collisions.DoCollision(Level.GetTilePolygon(x, y), this.polygon) &&
                                collidableEdges.Contains(TileEdge.Top))
                            {
                                // The player's center must be on the tile, not just a corner.
                                if (collision == TileCollision.SlantedUp &&
                                    x * Tile.Width < position.X && position.X <= (x + 1) * Tile.Width)
                                {
                                    // The player's Y position is a linear interpolation of the player's X position
                                    // relative to the tile.
                                    float relativeY = position.X - x * Tile.Width;
                                    relativeY /= Tile.Width;
                                    position.Y = MathHelper.Lerp(y * Tile.Height, (y + 1)
                                        * Tile.Height, 1 - relativeY) - 1;

                                    isOnGround = true;
                                    isOnRamp = true;
                                    velocity.Y = 0;
                                }
                                if (collision == TileCollision.SlantedDown &&
                                    x * Tile.Width <= position.X && position.X < (x + 1) * Tile.Width)
                                {
                                    // The player's Y position is a linear interpolation of the player's X position
                                    // relative to the tile.
                                    float relativeY = position.X - x * Tile.Width;
                                    relativeY /= Tile.Width;
                                    position.Y = MathHelper.Lerp(y * Tile.Height, (y + 1) * Tile.Height, relativeY) - 1;

                                    isOnGround = true;
                                    isOnRamp = true;
                                    velocity.Y = 0;
                                }
                            }
                        }
                        else if ((absDepthY < absDepthX || collision == TileCollision.Platform
                            || (!collidableEdges.Contains(TileEdge.Left) && depth.X < 0)
                            || (!collidableEdges.Contains(TileEdge.Right) && depth.X > 0))
                            && !isOnRamp && collision != TileCollision.SlantedUp && collision != TileCollision.SlantedDown)
                        // Resolve the collision along the shallow axis.
                        {
                            // If we crossed the top of a tile, or we were on a ramp and we haven't 
                            // jumped, we are on the ground.
                            if ((previousBottom <= tileBounds.Top &&
                                    collision != TileCollision.SlantedDown &&
                                    collision != TileCollision.SlantedDown) ||
                                wasOnRamp && !isJumping)
                            {
                                isOnGround = true;
                                position.Y = (int)Math.Round(position.Y);
                            }

                            // Ignore platforms, unless we are on the ground.
                            if ((collision == TileCollision.Impassable) ||
                                (IsOnGround && collidableEdges.Contains(TileEdge.Top)))
                            {
                                // Resolve the collision along the Y axis.
                                position.Y += depth.Y;
                                velocity.Y = 0;

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                        else if (collision == TileCollision.Impassable) // Ignore platforms.
                        {
                            if ((depth.X < 0 && collidableEdges.Contains(TileEdge.Left))
                                || (depth.X > 0 && collidableEdges.Contains(TileEdge.Right)))
                            {
                                // Resolve the collision along the X axis.
                                position.X += depth.X;

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }


            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, float elapsed)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if (IsOnGround || jumpTime > 0.0f)
                {
                    jumpTime += elapsed;
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity so that 
                    // the player hass more control over the jump
                    velocityY = (1 - jumpTime / MaxJumpTime) * JumpLaunchVelocity;
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }

            wasJumping = isJumping;
            return velocityY;
        }

        #endregion

        #region Triggered Events

        /// <summary>
        /// Called when the player is damaged by an enemy.
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isFinishing)
                return;

            if (isRageMode)
                damage *= .5f;

            if (isAlive)
            {
                health -= damage;
                healthRegenTime = 0f;
                health = MathHelper.Max(health, 0);

                if (Level.SurvivalMode)
                    One_Man_Army_Game.SurvivalGameData.DamageTaken += damage;
                else
                    One_Man_Army_Game.CampaignGameData.DamageTaken += damage;

                if (health <= 0.001f)
                {
                    OnKilled(null);
                    return;
                }
                if (Health < currentMaxHealth - 0.5f)
                {
                    currentMaxHealth -= 0.25f;
                    TakeDamage(0);
                }
            }
        }

        /// <summary>
        /// Adds power to the Rage bar.
        /// </summary>
        /// <param name="toAdd"></param>
        public void AddRage(float toAdd)
        {
            if (!isRageMode && !isRageBarFull)
            {
                rageDegradeGraceTime = 0f;
                rage += toAdd * RageMultiplyer / (float)Math.Pow(level.Wave, .5);
            }
            if (rage >= 1f)
            {
                isRageBarFull = true;
                rage = 1f;
            }
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;
            health = 0f;
            sprite.PlayAnimation(dieAnimation);

            if (Level.SurvivalMode)
                One_Man_Army_Game.SurvivalGameData.Deaths++;
            else
                One_Man_Army_Game.CampaignGameData.Deaths++;
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(idleAnimation);
        }

        /// <summary>
        /// Called when the player touches a power up on the map.
        /// </summary>
        /// <param name="powerUp"></param>
        public bool TriggerPowerUp(PowerUp powerUp)
        {
            switch (powerUp.Type)
            {
                case PowerUpType.Weapon:

                    AddWeapon(powerUp.Weapon);
                    return true;

                case PowerUpType.HealthPack:

                    if (health < maxHealth)
                    {
                        health = maxHealth;
                        currentMaxHealth = maxHealth;
                        level.Screen.HUD.AddHealth();
                        return true;
                    }

                    break;

                case PowerUpType.Repair:

                    level.RepairAll();
                    return true;

                case PowerUpType.Ammo:

                    if (weapons[currentWeapon].Ammo < weapons[currentWeapon].AmmoPerClip * 3)
                    {
                        AddWeapon(weapons[currentWeapon]);
                        return true;
                    }

                    break;

                case PowerUpType.HealthIncrease:

                    maxHealth += .25f;
                    currentMaxHealth = maxHealth;
                    health = maxHealth;
                    level.Screen.HUD.AddHealth();
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Player picks up a weapon.
        /// </summary>
        private void AddWeapon(Weapon weapon)
        {
            foreach (Weapon wep in weapons)
            {
                if (wep.Name == weapon.Name)
                {
                    wep.Ammo = Math.Min(wep.Ammo + wep.AmmoPerClip, wep.AmmoPerClip * 3);
                    return;
                }
            }

            level.Game.AllWeapons[weapon.Name].Activate();
            weapons.Add(level.Game.AllWeapons[weapon.Name]);
            currentWeapon = weapons.Count - 1;
            SwitchWeapon();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Called each frame, checks to see if the player's weapon is being fired and fires when appropriate.
        /// </summary>
        private void DoFireWeapon(float elapsed)
        {
            fireTime += elapsed;

            if (isFiring)
            {
                if (fireTime >= weapons[currentWeapon].FireRate)
                {
                    fireTime -= weapons[currentWeapon].FireRate;
                    while (fireTime >= elapsed)
                        fireTime -= elapsed;


                    Vector2 bulletPos = ArmPosition + armDirection * 56;

                    Bullet[] bulletsToAdd = weapons[currentWeapon].Fire(bulletPos, armDirection, this.Level, true);

                    if (bulletsToAdd != null)
                    {
                        for (int i = 0; i < bulletsToAdd.GetLength(0); i++)
                        {
                            level.PlayerBullets.Add(bulletsToAdd[i]);
                        }
                        weaponSprite.PlayAnimation(fireAnimation);
                        weaponSprite.Restart();
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the player can perform a finishing move at the moment.
        /// Used to decide when to draw the "Press Y" texture.
        /// </summary>
        private bool CanFinish(out Vector2 position)
        {
            position = Vector2.Zero;

            foreach (Enemy e in Level.Enemies)
            {
                if (e is Tank && e.State == EnemyState.Alive && (e.Health <= e.TotalHealth * 0.25 || e.Health <= 0.25f) &&
                    Math.Abs(e.Center.Y - this.Position.Y) < 48 && Math.Abs(e.Center.X - this.Position.X) < 128)
                {
                    Tank t = e as Tank;
                    position = new Vector2(t.Center.X + level.Random.Next(-1, 2),
                        t.InnerBounds.Top - t.InnerBounds.Height * 2 + level.Random.Next(-1, 2));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Switches weapons, updating the animation player.
        /// </summary>
        private void SwitchWeapon()
        {
            armsAnimation = new Animation(weapons[currentWeapon].ArmsTexture,
                weapons[currentWeapon].AnimationLength, false, 2f);
            fireAnimation = new Animation(weapons[currentWeapon].FireAnimation,
                weapons[currentWeapon].AnimationLength, false, 2f);

            weaponSprite.PlayAnimation(armsAnimation);
        }

        #endregion
    }
}