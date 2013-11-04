using  System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using System.IO;

namespace One_Man_Army
{
    public enum TimeOfDay
    {
        Dawn = 0,
        Midday = 1,
        Dusk = 2,
        Night = 3,
        Dawn2 = 4
    }

    public enum GameState
    {
        InGame = 0,
        InBetweenWaves = 1,
        InSupplyDrop = 2,
        InTransition = 3,
        InCutscene = 4
    }

    /// <summary>
    /// A uniform grid of tiles with collections of enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    public class Level : IDisposable
    {
        #region Fields

        public bool SurvivalMode;

        string loadPath;

        // TODO: SOMETHING WITH THIS
        private Texture2D whiteTex;
        private Texture2D kaboomTex;
        private Texture2D healthTex;
        private Texture2D healthBoostTex;
        private Texture2D ammoTex;
        private Texture2D repairTex;

        // Physical structure of the level
        private Tile[,] tiles;
        public Tile[,] Tiles
        {
            get { return tiles; }
            set { tiles = value; }
        }

        // Layers of the level's graphics.
        private Layer[] layers;
        private SmokePlumeInstance[] plumes;

        // The position of the Camera.
        public float CameraPosition
        {
            get { return cameraPosition; }
        }
        private float cameraPosition;

        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // The game parent of this level.
        public One_Man_Army_Game Game
        {
            get { return game; }
        }
        One_Man_Army_Game game;

        // The screen parent of this level.
        public GameplayScreen Screen
        {
            get { return screen; }
        }
        GameplayScreen screen;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
            set { player = value; }
        }
        Player player;

        // Can the player move?
        public bool PlayerHasControl
        {
            get { return !(timeTilNextSpawn > 10 && timeTilNextSpawn <= 12); }
        }

        private List<PowerUp> powerUps = new List<PowerUp>();

        // Key locations in the level.        
        private Vector2 start;
        public Vector2 Start
        {
            get { return start; }
            set { start = value; }
        }
        private static readonly Point InvalidPosition = new Point(-1, -1);

        /// <summary>
        /// The outer bounds of the level.
        /// </summary>
        Rectangle levelBounds;
        public Rectangle LevelBounds
        {
            get { return levelBounds; }
            set { levelBounds = value; }
        }

        /// <summary>
        /// A random number for use by all of the level's components.
        /// </summary>
        public Random Random;

        /// <summary>
        /// The wave of enemies the player is facing.
        /// </summary>
        public int Wave
        {
            get { return wave; }
        }
        int wave;

        /// <summary>
        /// The current time of day (dawn, midday, dusk, or night).
        /// </summary>
        public TimeOfDay CurrentTime
        {
            get { return currentTime; }
        }
        TimeOfDay currentTime = TimeOfDay.Dawn;

        /// <summary>
        /// Level content.
        /// </summary>     
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        // Particle Systems

        public ExplosionParticleSystem Explosion
        {
            get { return explosion; }
        }
        ExplosionParticleSystem explosion;

        public ExplosionSmokeParticleSystem ExplosionSmoke
        {
            get { return explosionSmoke; }
        }
        ExplosionSmokeParticleSystem explosionSmoke;

        public SmokeEmitterParticleSystem SmokeTrails
        {
            get { return smokeTrails; }
        }
        SmokeEmitterParticleSystem smokeTrails;

        public TileExplosionParticleSystem TileExplosion
        {
            get { return tileExplosion; }
        }
        TileExplosionParticleSystem tileExplosion;

        public TextParticleSystem WeaponText
        {
            get { return weaponText; }
        }
        TextParticleSystem weaponText;

        public SmokePlumeParticleSystem SmokePlumes1
        {
            get { return smokePlumes1; }
        }
        SmokePlumeParticleSystem smokePlumes1;

        public SmokePlumeParticleSystem SmokePlumes2
        {
            get { return smokePlumes2; }
        }
        SmokePlumeParticleSystem smokePlumes2;

        /// <summary>
        /// The maximum number of bullets on the screen
        /// </summary>
        const int MaxNumBullets = 2000;

        // Queues, for optimal performance.

        private Queue<Bullet> bulletQueue = new Queue<Bullet>();
        private Queue<Tank> tankQueue = new Queue<Tank>();
        private Queue<Heli> heliQueue = new Queue<Heli>();
        private Queue<PowerUp> powerUpQueue = new Queue<PowerUp>();

        /// <summary>
        /// A queue holding all bullets not in use.
        /// </summary>
        public Queue<Bullet> BulletQueue
        {
            get { return bulletQueue; }
            set { bulletQueue = value; }
        }

        /// <summary>
        /// All enemies active in the level.
        /// </summary>
        public List<Enemy> Enemies
        {
            get { return enemies; }
        }
        private List<Enemy> enemies = new List<Enemy>();

        /// <summary>
        /// All enemy spawn points in the level.
        /// </summary>
        public List<EnemySpawnPoint> EnemySpawnPoints
        {
            get { return enemySpawnPoints; }
        }
        private List<EnemySpawnPoint> enemySpawnPoints = new List<EnemySpawnPoint>();

        /// <summary>
        /// The amount of time between enemy spawns; enemies do not respawn instantly.
        /// </summary>
        const int WaveTime = 3;
        float timeTilNextSpawn = 5;
        float lastTimeTilNextSpawn = 5;
        float timeRemaining = 60f;

        public float TimeTilNextSpawn
        {
            get { return timeTilNextSpawn; }
        }

        public float TimeRemaining
        {
            get { return timeRemaining; }
        }

        /// <summary>
        /// The probability that an enemy will drop a power-up.
        /// </summary>
        public const int DropProbability = 2;

        /// <summary>
        /// Is the level between waves?
        /// </summary>
        public bool IsBetweenWaves
        {
            get { return timeTilNextSpawn > WaveTime && timeTilNextSpawn <= WaveTime + 2; }
        }

        public GameState CurrentState
        {
            get
            {
                if (timeRemaining <= 0)
                    return GameState.InCutscene;

                if (timeTilNextSpawn <= WaveTime)
                    return GameState.InGame;

                if (timeTilNextSpawn > WaveTime && timeTilNextSpawn <= WaveTime + 2)
                    return GameState.InBetweenWaves;

                if (timeTilNextSpawn >= WaveTime + 2 && currentTime == (TimeOfDay)(wave / 6))
                    return GameState.InSupplyDrop;

                if ((wave - 1) % 6 == 0 && currentTime != (TimeOfDay)(wave / 6))
                    return GameState.InTransition;

                return GameState.InGame;
            }
        }

        GameState lastGameState;

        /// <summary>
        /// All hostile bullets in the level.
        /// </summary>
        public List<Bullet> EnemyBullets
        {
            get { return enemyBullets; }
            set { enemyBullets = value; }
        }
        private List<Bullet> enemyBullets = new List<Bullet>();

        /// <summary>
        /// All friendly bullets in the level.
        /// </summary>
        public List<Bullet> PlayerBullets
        {
            get { return playerBullets; }
            set { playerBullets = value; }
        }
        private List<Bullet> playerBullets = new List<Bullet>();

        /// <summary>
        /// The number of tanks remaining in the level.
        /// </summary>
        public int TanksToKill
        {
            get { return tanksToKill; }
        }
        int tanksToKill;

        /// <summary>
        /// The number of helis remaining in the level.
        /// </summary>
        public int HelisToKill
        {
            get { return helisToKill; }
        }
        int helisToKill;

        private HeliDataHolder[] heliData;
        private TankDataHolder[] tankData;

        // Max number of enemies in the level at any given time.
        int maxTanksOnMap;
        int maxHelisOnMap;

        /// <summary>
        /// The volume of the music, as far as external classes can control it.
        /// </summary>
        public float PublicMusicVolume
        {
            get { return publicMusicVolume; }
            set 
            { 
                publicMusicVolume = value; 
                MusicVolume = musicVolume; 
            }
        }
        private float publicMusicVolume = 1f;

        /// <summary>
        /// The volume of the music, as far as the level can control it.
        /// </summary>
        private float MusicVolume
        {
            get { return musicVolume; }
            set
            {
                musicVolume = value;
                
                AudioCategory category = game.AudioEngine.GetCategory("Music");
                category.SetVolume(((float)(OptionsMenuScreen.MusicVolume) / 200f) * musicVolume * PublicMusicVolume);
            }
        }
        private float musicVolume = 1f;

        #region Performance monitors

        Stopwatch TotalSW;
        Stopwatch PlayerSW;
        Stopwatch EnemySW;
        Stopwatch BulletSW;
        Stopwatch ParticleSW;

        public int TotalTime;
        public int PlayerTime;
        public int EnemyTime;
        public int BulletTime;
        public int ParticleTime;

        float timeSinceLastMonitor = 0;

        #endregion

        #endregion

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="path">
        /// The absolute path to the level file to be loaded.
        /// </param>
        public Level(IServiceProvider serviceProvider, string path, One_Man_Army_Game g,
            GameplayScreen s, int startWave, bool survival)
        {
            Random = new Random();
            loadPath = path;
            game = g;
            screen = s;
            wave = startWave;
            SurvivalMode = survival;

            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            LoadContent();
        }

        /// <summary>
        /// Loads all the content for the level.
        /// </summary>
        public void LoadContent()
        {
            int numHelis = 3;
            int numTanks = 3;

            LevelLoader.Reset();

            // Load miscellaneous textures. Really should be cleaner.
            whiteTex = Content.Load<Texture2D>("white"); 
            kaboomTex = Content.Load<Texture2D>("Sprites/Particles/kaboom");
            ammoTex = Content.Load<Texture2D>("Sprites/Power Ups/ammo");
            healthTex = Content.Load<Texture2D>("Sprites/Power Ups/health");
            healthBoostTex = Content.Load<Texture2D>("Sprites/Power Ups/health boost");
            repairTex = Content.Load<Texture2D>("Sprites/Power Ups/repair");

            // Load particle systems
            explosionSmoke = new ExplosionSmokeParticleSystem(this.Game, 100);
            explosionSmoke.Initialize();

            explosion = new ExplosionParticleSystem(this.Game, 100);
            explosion.Initialize();

            tileExplosion = new TileExplosionParticleSystem(this.Game, 5);
            tileExplosion.Initialize();

            smokeTrails = new SmokeEmitterParticleSystem(this.Game, 25);
            smokeTrails.Initialize();

            weaponText = new TextParticleSystem(this.Game, 25);
            weaponText.Initialize();

            int shadeColor = ((wave / 6) % 4);
            if (shadeColor == 0)
                shadeColor += 2;

            float timeShade = 0.5f - shadeColor * 0.13f;

            currentTime = SurvivalMode ? (TimeOfDay)((wave / 6) % 4) : (TimeOfDay)(wave / 6);

            Color color = new Color(timeShade, timeShade, timeShade);
            smokePlumes1 = new SmokePlumeParticleSystem(this.Game, 100, color);
            smokePlumes1.Initialize();

            timeShade += 0.15f;
            color = new Color(timeShade, timeShade, timeShade);
            smokePlumes2 = new SmokePlumeParticleSystem(this.Game, 100, color);
            smokePlumes2.Initialize();

            // Load background layers
            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/" + currentTime.ToString() + "/Layer2", 0.2f);

            if (SurvivalMode)
            {
                layers[1] = new Layer(Content, "Backgrounds/Night/Layer1", 0.5f);
                layers[2] = new Layer(Content, "Backgrounds/Night/Layer0", 0.8f);
            }
            else
            {
                layers[1] = new Layer(Content, "Backgrounds/" + currentTime.ToString() + "/Layer1", 0.5f);
                layers[2] = new Layer(Content, "Backgrounds/" + currentTime.ToString() + "/Layer0", 0.8f);
            }

            // Load tiles, bullets
            LevelLoader.LoadTiles(this, ref tiles, loadPath);

            // The number of plumes will increse as time passes during the campaign, or bemaxed out in Survival mode.
            int numPlumes;
            if (SurvivalMode)
                numPlumes = 6;
            else
                numPlumes = Math.Min(currentTime == TimeOfDay.Dawn ? 0 : (int)currentTime * 2, 6);

            plumes = new SmokePlumeInstance[numPlumes];

            for (int i = 0; i < numPlumes; i++)
            {
                bool backOrFront = i % 2 == 1;
                Vector2 possibleRange = new Vector2((float)((1280 + (Width * Tile.Width - 1280) * (backOrFront ? 0.35 : 0.65))
                    * (float)(i / 2) / (float)(numPlumes / 2)), (float)((1280 + (Width * Tile.Width - 1280) * (backOrFront ? 0.35 : 0.65))
                    * (float)((i + 2) / 2) / (float)(numPlumes / 2)));

                Vector2 pos = new Vector2(One_Man_Army_Game.RandomBetween(possibleRange.X, possibleRange.Y), (Height - 1) * Tile.Height);

                SmokePlumeInstance plume = new SmokePlumeInstance(backOrFront ? smokePlumes1 : smokePlumes2,
                    pos, 128, this);

                plumes[i] = plume;
            }

            float t = 0;
            do
            {
                float elapsed = (1f / 60f);
                t += elapsed;
                smokePlumes1.Update(elapsed);
                smokePlumes2.Update(elapsed);

                for (int i = 0; i < plumes.Length; i++)
                {
                    plumes[i].Update(elapsed);
                }
            }
            while (t < 5f);

            for (int i = 0; i < MaxNumBullets; i++)
            {
                Bullet b = new Bullet();
                bulletQueue.Enqueue(b);
            }

            heliData = new HeliDataHolder[numHelis];
            for (int i = 0; i < numHelis; i++)
            {
                heliData[i] = LevelLoader.LoadHeliData("Heli" + i);
            }

            tankData = new TankDataHolder[numTanks];
            for (int i = 0; i < numTanks; i++)
            {
                tankData[i] = LevelLoader.LoadTankData("Tank" + i);
            }

            if (powerUpQueue.Count == 0)
                EnqueuePowerUp(false);

            // Initialize stopwatches... no longer used, may be reinstated at some point. keep.
            TotalSW = new Stopwatch();
            PlayerSW = new Stopwatch();
            EnemySW = new Stopwatch();
            BulletSW = new Stopwatch();
            ParticleSW = new Stopwatch();

            LoadNextWave();

            if (screen.MusicCue != null)
                screen.MusicCue.Dispose();
            screen.MusicCue = Game.MusicBank.GetCue(currentTime.ToString() + " Music");
        }

        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                if (y < -2 || y > Height + 3)
                    return TileCollision.Impassable;
                else
                    return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        /// <summary>
        /// Gets the tile collision mode at a specific Vector2, in world coordinates.
        /// </summary>
        public TileCollision GetCollision(Vector2 WorldPosition)
        {
            int x = (int)(WorldPosition.X / 64);
            int y = (int)(WorldPosition.Y / 48);

            if (WorldPosition.X < 0)
                x -= 1;
            if (WorldPosition.Y < 0)
                y -= 1;

            return GetCollision(x, y);
        }

        /// <summary>
        /// Returns an int X and int Y for the tile at the Vector2 
        /// given in world coordinates.
        /// </summary>
        public Vector2 GetTilePosition(Vector2 WorldPosition)
        {
            int x = (int)(WorldPosition.X / 64);
            int y = (int)(WorldPosition.Y / 48);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns a list of all the collidable edges of a specifies tile.
        /// </summary>
        public List<TileEdge> GetCollidableEdges(int x, int y)
        {
            List<TileEdge> edges = new List<TileEdge>();

            if (GetCollision(x - 1, y) != TileCollision.Impassable &&
                GetCollision(x - 1, y) != TileCollision.SlantedUp)
                edges.Add(TileEdge.Left);

            if (GetCollision(x, y - 1) != TileCollision.Impassable &&
                GetCollision(x, y - 1) != TileCollision.SlantedUp &&
                GetCollision(x, y - 1) != TileCollision.SlantedDown &&
                GetCollision(x, y - 1) != TileCollision.Platform)
                edges.Add(TileEdge.Top);

            if (GetCollision(x + 1, y) != TileCollision.Impassable &&
                GetCollision(x + 1, y) != TileCollision.SlantedDown)
                edges.Add(TileEdge.Right);

            if (GetCollision(x, y + 1) != TileCollision.Impassable)
                edges.Add(TileEdge.Bottom);

            return edges;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>
        public Rectangle GetTileBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Returns a collision object for the tile - rectangles for standard tiles, triangles for ramps.
        /// </summary>
        public Polygon GetTilePolygon(int x, int y)
        {
            List<Vector2> vertices = new List<Vector2>();

            if ((int)GetCollision(x, y) <= 3)
            {
                vertices.Add(Vector2.Zero);
                vertices.Add(new Vector2(Tile.Width, 0));
                vertices.Add(new Vector2(Tile.Width, Tile.Height));
                vertices.Add(new Vector2(0, Tile.Height));
            }
            else if (GetCollision(x, y) == TileCollision.SlantedUp)
            {
                vertices.Add(new Vector2(Tile.Width, 0));
                vertices.Add(new Vector2(Tile.Width, Tile.Height));
                vertices.Add(new Vector2(0, Tile.Height));
            }
            else
            {
                vertices.Add(Vector2.Zero);
                vertices.Add(new Vector2(Tile.Width, Tile.Height));
                vertices.Add(new Vector2(0, Tile.Height));
            }

            Polygon p = Polygon.MakePolygon(vertices);
            p.Position = new Vector2(x * Tile.Width, y * Tile.Height);
            return p;
        }

        /// <summary>
        /// Restores all destroyed tiles in the level to their original states.
        /// </summary>
        public void RepairAll()
        {
            LevelLoader.LoadTiles(this, ref tiles, loadPath);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        #region Main

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(float elapsed)
        {
            timeSinceLastMonitor += elapsed;
            if (SurvivalMode)
                One_Man_Army_Game.SurvivalGameData.TimePlayed += TimeSpan.FromSeconds(elapsed);
            else
                One_Man_Army_Game.CampaignGameData.TimePlayed += TimeSpan.FromSeconds(elapsed);

            if (timeSinceLastMonitor > 2f)
            {
                TotalTime = 0;
                PlayerTime = 0;
                EnemyTime = 0;
                BulletTime = 0;
                ParticleTime = 0;

                timeSinceLastMonitor = 0;
            }

            TotalSW.Reset();
            PlayerSW.Reset();
            EnemySW.Reset();
            BulletSW.Reset();
            ParticleSW.Reset();

            TotalSW.Start();

            // Slo-mo while the player is dead or time is expired.
            if (!Player.IsAlive )
            {
                elapsed *= 0.5f;

                explosionSmoke.Update(elapsed);
                explosion.Update(elapsed);
                tileExplosion.Update(elapsed);
                smokeTrails.Update(elapsed);
                WeaponText.Update(elapsed);

                UpdateEnemies(elapsed);
                UpdatePowerUps(elapsed);
                UpdateBullets(elapsed);

                Player.Update(elapsed);
            }
            else
            {
                if (player.IsRageMode)
                    elapsed *= 0.5f;

                ParticleSW.Start();

                explosionSmoke.Update(elapsed);
                explosion.Update(elapsed);
                tileExplosion.Update(elapsed);
                smokeTrails.Update(elapsed);
                weaponText.Update(elapsed);
                smokePlumes1.Update(elapsed);
                smokePlumes2.Update(elapsed);
                for (int i = 0; i < plumes.Length; i++)
                    plumes[i].Update(elapsed);

                ParticleSW.Stop();

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    Player.OnKilled(null);

                EnemySW.Start();
                UpdateEnemies(elapsed);
                EnemySW.Stop();

                UpdatePowerUps(elapsed);

                BulletSW.Start();
                UpdateBullets(elapsed);
                BulletSW.Stop();

                PlayerSW.Start();
                Player.Update(elapsed);
                PlayerSW.Stop();

                UpdateWaves(elapsed);

                lastGameState = CurrentState;
            }

            TotalSW.Stop();

            TotalTime = (int)MathHelper.Max(TotalSW.ElapsedTicks, TotalTime);
            PlayerTime = (int)MathHelper.Max(PlayerSW.ElapsedTicks, PlayerTime);
            EnemyTime = (int)MathHelper.Max(EnemySW.ElapsedTicks, EnemyTime);
            BulletTime = (int)MathHelper.Max(BulletSW.ElapsedTicks, BulletTime);
            ParticleTime = (int)MathHelper.Max(ParticleSW.ElapsedTicks, ParticleTime);
        }

        #endregion

        #region Bullets

        /// <summary>
        /// Updates each projectile in the level.
        /// </summary>
        private void UpdateBullets(float elapsed)
        {
            List<BulletCollisionCircle> playerCollisionCircles = new List<BulletCollisionCircle>();
            List<BulletCollisionCircle> enemyCollisionCircles = new List<BulletCollisionCircle>();

            for (int i = 0; i < playerBullets.Count; i++)
            {
                playerBullets[i].Update(this, elapsed);

                BulletCollisionCircle collisionCircle = playerBullets[i].CheckForCollisionsWithEnemies(this);

                if (collisionCircle.circle != null)
                {
                    playerCollisionCircles.Add(collisionCircle);
                    if (playerBullets[i].CollisionCircle.radius < playerBullets[i].DamageRadius)
                    {
                        explosion.AddParticles(collisionCircle.circle.Position, playerBullets[i].DamageRadius);
                        explosionSmoke.AddParticles(collisionCircle.circle.Position, playerBullets[i].DamageRadius);
                    }

                    bulletQueue.Enqueue(playerBullets[i]);
                    playerBullets.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < enemyBullets.Count; i++)
            {
                enemyBullets[i].Update(this, elapsed);

                BulletCollisionCircle collisionCircle = enemyBullets[i].CheckForCollisionsWithPlayer(this);

                if (collisionCircle.circle != null)
                {
                    enemyCollisionCircles.Add(collisionCircle);
                    if (enemyBullets[i].CollisionCircle.radius < enemyBullets[i].DamageRadius)
                    {
                        explosion.AddParticles(collisionCircle.circle.Position, enemyBullets[i].DamageRadius);
                        explosionSmoke.AddParticles(collisionCircle.circle.Position, enemyBullets[i].DamageRadius);
                    }

                    bulletQueue.Enqueue(enemyBullets[i]);
                    enemyBullets.RemoveAt(i);
                    i--;
                }
            }

            foreach (BulletCollisionCircle circle in playerCollisionCircles)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].State == EnemyState.Alive &&
                        Collisions.DoCollision(enemies[i].CollisionPolygon, circle.circle))
                    {
                        enemies[i].TakeDamage(circle.damage);
                        screen.SFXManager.PlayCue((enemies[i] is Tank ? "Tank" : "Heli") + " Damage");
                        player.AddRage(circle.damage);

                        if (SurvivalMode)
                            One_Man_Army_Game.SurvivalGameData.DamageDealt += circle.damage;
                        else
                            One_Man_Army_Game.CampaignGameData.DamageDealt += circle.damage;
                    }
                }

                DoBulletCollisionWithTiles(circle);
            }

            foreach (BulletCollisionCircle circle in enemyCollisionCircles)
            {
                if (Collisions.DoCollision(player.CollisionPolygon, circle.circle))
                {
                    player.TakeDamage(circle.damage);
                }

                DoBulletCollisionWithTiles(circle);
            }
        }

        #endregion

        #region Enemies

        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(float elapsed)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update(elapsed);

                if (enemies[i].State == EnemyState.Dead)
                {
                    if (enemies[i] is Tank)
                    {
                        tanksToKill--;

                        if (SurvivalMode)
                            One_Man_Army_Game.SurvivalGameData.TanksKilled++;
                        else
                            One_Man_Army_Game.CampaignGameData.TanksKilled++;

                        Tank t = enemies[i] as Tank;
                        t.MoveCue.Stop(AudioStopOptions.Immediate);
                        screen.SFXManager.PlayCue("Tank Death");
                        player.AddRage(t.MaxHealth * .5f);
                        tankQueue.Enqueue(t);
                    }

                    if (enemies[i] is Heli)
                    {
                        helisToKill--;

                        if (SurvivalMode)
                            One_Man_Army_Game.SurvivalGameData.HelisKilled++;
                        else
                            One_Man_Army_Game.CampaignGameData.HelisKilled++;

                        Heli h = enemies[i] as Heli;
                        screen.SFXManager.PlayCue("Heli Death");
                        player.AddRage(h.MaxHealth * .5f);
                        heliQueue.Enqueue(h);
                    }

                    if (tanksToKill <= 0 && helisToKill <= 0)
                        LoadNextWave();

                    for (int j = 0; j < enemySpawnPoints.Count; j++)
                        if (enemies[i].SpawnPoint == enemySpawnPoints[j])
                            enemySpawnPoints[j].isEnemyActive = false;

                    if (Random.Next(DropProbability) == 0)
                    {
                        EnqueuePowerUp(false);
                        SpawnPowerUp(enemies[i].Position, enemies[i] is Tank ? true : false);
                    }

                    explosionSmoke.AddParticles(enemies[i].CollisionPolygon.Center,
                        enemies[i].CollisionPolygon.Radius * 1.5f);

                    explosion.AddParticles(enemies[i].CollisionPolygon.Center,
                        enemies[i].CollisionPolygon.Radius * 1.5f);

                    weaponText.AddParticles(enemies[i].CollisionPolygon.Center,
                        enemies[i].CollisionPolygon.Radius / 32,
                        kaboomTex);

                    enemies.RemoveAt(i);

                    break;
                }
            }
        }

        #endregion

        #region Power Ups

        private void UpdatePowerUps(float elapsed)
        {
            for (int i = 0; i < powerUps.Count; i++)
            {
                powerUps[i].Update(elapsed);

                if (!powerUps[i].IsAlive)
                {
                    powerUps.RemoveAt(i);
                    i--;
                    continue;
                }

                if (player.BoundingRectangle.Intersects(powerUps[i].BoundingRectangle))
                {
                    if (player.TriggerPowerUp(powerUps[i]))
                    {
                        powerUps.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        #endregion

        #region Waves

        private void UpdateWaves(float elapsed)
        {
            float lastTimeRemaining = timeRemaining;

            if ((wave == 25 && CurrentState == GameState.InGame && !SurvivalMode) || timeRemaining < 0)
                timeRemaining -= elapsed;

            if (timeRemaining <= 0 && lastTimeRemaining > 0)
                LoadFinalCutscene();

            if (timeRemaining <= -5f)
                EndGame();

            if (timeTilNextSpawn == 10f)
            {
                EnqueuePowerUp(true);

                if (CurrentState == GameState.InTransition && lastTimeTilNextSpawn < timeTilNextSpawn)
                    timeTilNextSpawn += 5f;
            }

            lastTimeTilNextSpawn = timeTilNextSpawn;
            timeTilNextSpawn -= elapsed;

            if (timeTilNextSpawn <= 12f && CurrentState == GameState.InTransition)
            {
                if (lastTimeTilNextSpawn > 12f)
                    DestroyAllBullets();

                if (timeTilNextSpawn > 10f)
                    MusicVolume = (timeTilNextSpawn - 10f) / 2f;
            }

            if (timeTilNextSpawn <= 10f && lastTimeTilNextSpawn > 10f && CurrentState == GameState.InTransition)
            {
                wave--;
                LoadNextTimePeriod();
            }

            if ((timeTilNextSpawn <= 9f && CurrentState == GameState.InSupplyDrop) || timeTilNextSpawn <= 4f)
                if (screen.MusicCue.IsPrepared && !screen.MusicCue.IsPlaying)
                    screen.MusicCue.Play();

            if (CurrentState == GameState.InSupplyDrop || lastGameState == GameState.InSupplyDrop)
            {
                Vector2 pos = new Vector2(CameraPosition + (1280f / 6f) * (10 - (lastTimeTilNextSpawn - lastTimeTilNextSpawn % 1)), 0);
                if (lastTimeTilNextSpawn % 1f < timeTilNextSpawn % 1f && lastTimeTilNextSpawn < 10)
                    SpawnPowerUp(pos, false);
            }

            if (timeTilNextSpawn <= 0)
            {
                timeTilNextSpawn = WaveTime;
                SpawnEnemies();
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Enemies

        /// <summary>
        /// Called once every five seconds. If the number of enemies on screen is less than the
        /// maximum number, spawn an enemy of the appropriate type. The method loops through the
        /// possible spawn points, and chooses one of the correct type based on preference. First, 
        /// it will try to spawn an enemy more than 400 units away from the player (so the player
        /// can't camp spawns). Then, it tries to spawn one no more than 1200 away from the player, 
        /// so the player won't have to move far to get in the action.
        /// </summary>
        private void SpawnEnemies()
        {
            int numTanks = 0;
            int numHelis = 0;
            foreach (Enemy e in enemies)
            {
                if (e is Tank)
                    numTanks++;
                else
                    numHelis++;
            }

            if (numTanks < maxTanksOnMap && numTanks < tanksToKill)
            {
                List<int> possibleSpawns = new List<int>();

                // Loop through the spawn points. An enemy may be spawned at any one without an active enemy.
                for (int i = 0; i < enemySpawnPoints.Count; i++)
                {
                    if (enemySpawnPoints[i].enemyType == typeof(Tank) &&
                        enemySpawnPoints[i].isEnemyActive == false &&
                        GetCollision(new Vector2(
                            enemySpawnPoints[i].location.X, enemySpawnPoints[i].location.Y + 32)) !=
                            TileCollision.Passable)
                        possibleSpawns.Add(i);
                }

                float furthestFromPlayer = 0f;
                foreach (int i in possibleSpawns)
                {
                    furthestFromPlayer = MathHelper.Max(furthestFromPlayer,
                        Vector2.Distance(this.Player.Position, enemySpawnPoints[i].location));
                }

                if (furthestFromPlayer > 400f)
                {
                    for (int i = 0; i < possibleSpawns.Count; i++)
                    {
                        if (Vector2.Distance(this.Player.Position, enemySpawnPoints[possibleSpawns[i]].location) < 400f)
                        {
                            possibleSpawns.Remove(possibleSpawns[i]);
                            i--;
                        }
                    }
                }

                float closestToPlayer = float.PositiveInfinity;
                foreach (int i in possibleSpawns)
                {
                    closestToPlayer = MathHelper.Min(closestToPlayer,
                        Vector2.Distance(this.Player.Position, enemySpawnPoints[i].location));
                }

                if (closestToPlayer < 1280f)
                {
                    for (int i = 0; i < possibleSpawns.Count; i++)
                    {
                        if (Vector2.Distance(this.Player.Position, enemySpawnPoints[possibleSpawns[i]].location) > 1280f)
                        {
                            possibleSpawns.Remove(possibleSpawns[i]);
                            i--;
                        }
                    }
                }

                int rnd = Random.Next(possibleSpawns.Count);

                Tank t = tankQueue.Dequeue();
                t.InitEnemy(enemySpawnPoints[possibleSpawns[rnd]].location,
                    enemySpawnPoints[possibleSpawns[rnd]], tankData[t.ID]);

                enemies.Add(t);

                enemySpawnPoints[possibleSpawns[rnd]].isEnemyActive = true;
                numTanks++;
            }

            if (numHelis < maxHelisOnMap && numHelis < helisToKill)
            {
                List<int> possibleSpawns = new List<int>();

                // Loop through the spawn points. An enemy may be spawned at any one without an active enemy.
                for (int i = 0; i < enemySpawnPoints.Count; i++)
                    if (enemySpawnPoints[i].enemyType == typeof(Heli) &&
                        enemySpawnPoints[i].isEnemyActive == false)
                        possibleSpawns.Add(i);

                float furthestFromPlayer = 0f;
                foreach (int i in possibleSpawns)
                {
                    furthestFromPlayer = MathHelper.Max(furthestFromPlayer,
                        Vector2.Distance(this.Player.Position, enemySpawnPoints[i].location));
                }

                if (furthestFromPlayer > 400f)
                {
                    for (int i = 0; i < possibleSpawns.Count; i++)
                    {
                        if (Vector2.Distance(this.Player.Position, enemySpawnPoints[possibleSpawns[i]].location) < 400f)
                        {
                            possibleSpawns.Remove(possibleSpawns[i]);
                            i--;
                        }
                    }
                }

                int rnd = Random.Next(possibleSpawns.Count);

                Heli h = heliQueue.Dequeue();
                h.InitEnemy(enemySpawnPoints[possibleSpawns[rnd]].location, 
                    enemySpawnPoints[possibleSpawns[rnd]], heliData[h.ID]);

                enemies.Add(h);

                enemySpawnPoints[possibleSpawns[rnd]].isEnemyActive = true;
                numHelis++;
            }
        }

        #endregion

        #region Power Ups

        /// <summary>
        /// Adds a random power-up, or five, to the queue (but only if it's not already enqueued)
        /// </summary>
        /// <param name="supplyDrop"></param>
        private void EnqueuePowerUp(bool supplyDrop)
        {
            int numToAdd = supplyDrop ? 5 : 1;
            int j = 5;

            if (supplyDrop)
            {
                powerUpQueue.Clear();
                j = Random.Next(4);
            }

            List<string> queuedPups = new List<string>();

            if (powerUpQueue.Count >= 1)
                queuedPups.Add(powerUpQueue.Peek().Weapon == null ?
                    powerUpQueue.Peek().Type.ToString() :
                    powerUpQueue.Peek().Weapon.Name);

            for (int i = 0; i < numToAdd; i++)
            {
                PowerUp toAdd = new PowerUp();

                if (i == j)
                {
                    toAdd = new PowerUp(PowerUpType.HealthIncrease, null, Vector2.Zero, this, healthBoostTex, false);
                    powerUpQueue.Enqueue(toAdd);
                    continue;
                }

                int r = Random.Next(6);
                if (supplyDrop)
                    r = Math.Max(r, 1);

                if (r == 0)
                    toAdd = new PowerUp(PowerUpType.HealthPack, null, Vector2.Zero, this, healthTex, false);
                if (r == 1)
                    toAdd = new PowerUp(PowerUpType.Ammo, null, Vector2.Zero, this, ammoTex, false);
                if (r == 2)
                    toAdd = new PowerUp(PowerUpType.Repair, null, Vector2.Zero, this, repairTex, false);

                if (r >= 3)
                {
                    int wepCap = Math.Min(wave - (int)currentTime * 2,
                        ((int)currentTime + 1) * 4);

                    int k = Random.Next(wepCap) + 1;
                    k = Math.Max(
                        Math.Min(k, Weapon.NumWeapons - 1),
                        Math.Min(wepCap, Weapon.NumWeapons - 1) - (Weapon.NumWeapons / Weapon.NumTiers) + 1);

                    Weapon wep = Game.AllWeapons[Game.WeaponKeys[k]].Copy();
                    toAdd = new PowerUp(PowerUpType.Weapon, wep, Vector2.Zero, this, wep.WeaponTexture, false);
                }

                string id = toAdd.Weapon == null ? toAdd.Type.ToString() : toAdd.Weapon.Name;
                if (queuedPups.Contains(id))
                    i--;
                else
                {
                    powerUpQueue.Enqueue(toAdd);
                    queuedPups.Add(id);
                }
            }
        }

        /// <summary>
        /// Spawns one power-up or weapon drop on the map.
        /// </summary>
        /// <param name="position">The power-up's initial position</param>
        /// <param name="onGround">Is the power-up initially on the ground?</param>
        private void SpawnPowerUp(Vector2 position, bool onGround)
        {
            PowerUp Pup = powerUpQueue.Dequeue();
            PowerUp newPup = new PowerUp(
                Pup.Type, Pup.Weapon, position, this, Pup.Texture, onGround);
            powerUps.Add(newPup);
        }

        #endregion

        #region Waves

        /// <summary>
        /// Loads the next wave of enemies.
        /// </summary>
        private void LoadNextWave()
        {
            wave++;

            if (Guide.IsTrialMode)
            {
                if (wave > 5)
                {
                    screen.ScreenManager.AddScreen(new PurchaseMenuScreen(), screen.ControllingPlayer);
                }
            }
            else
            {
                if (SurvivalMode)
                {
                    One_Man_Army_Game.SurvivalGameData.MaxWave = Math.Max(One_Man_Army_Game.SurvivalGameData.MaxWave, Math.Min(wave - 1, 24));
                    Game.SaveGameData(One_Man_Army.One_Man_Army_Game.SurvivalGameData, One_Man_Army_Game.FileName_Game_Survival);
                }
                else
                {
                    One_Man_Army_Game.CampaignGameData.MaxWave = Math.Max(One_Man_Army_Game.CampaignGameData.MaxWave, Math.Min(wave - 1, 24));
                    Game.SaveGameData(One_Man_Army.One_Man_Army_Game.CampaignGameData, One_Man_Army_Game.FileName_Game_Campaign);
                }
            }

            // This variable scales back the actual wave at the beginning of each new time period
            // so that the difficulty curve is softer than it seems in the campaign. It is not present
            // in Survival mode.
            int scaledWave = SurvivalMode ? wave : wave - (wave / 6) * 2;

            maxTanksOnMap = Math.Min(1 + (scaledWave + 2) / 6, 3);
            maxHelisOnMap = Math.Min(1 + (scaledWave - 1) / 5, 2);
            int possibleTanks = Math.Min(1 + (scaledWave) / 3, 3);
            int possibleHelis = Math.Min(1 + (scaledWave) / 3, 3);

            // During the last level of the campaign, the player can kill as many enemies as possible.
            if (wave < 25 || SurvivalMode)
            {
                tanksToKill = 4 + (2 * scaledWave) / 3 - maxTanksOnMap * 2;
                helisToKill = 3 + scaledWave / 2 - maxHelisOnMap * 2;
            }
            else
            {
                tanksToKill = int.MaxValue;
                helisToKill = int.MaxValue;
            }

            for (int i = 0; i < maxTanksOnMap; i++)
            {
                int rnd = Random.Next(possibleTanks);
                Tank t = new Tank(this, rnd);
                tankQueue.Enqueue(t);
            }

            for (int i = 0; i < maxHelisOnMap; i++)
            {
                int rnd = Random.Next(possibleHelis);
                Heli h = new Heli(this, rnd);
                heliQueue.Enqueue(h);
            }

            timeTilNextSpawn = 5;
            if ((wave - 1) % 3 == 0 && wave > 1)
                timeTilNextSpawn += 5;
        }

        /// <summary>
        /// Called when a new background must be loaded, every 6 waves.
        /// </summary>
        private void LoadNextTimePeriod()
        {
            SplashScreen splash = new SplashScreen((currentTime != TimeOfDay.Dawn2 && !SurvivalMode), true, (int)currentTime + 1);

            splash.BackgroundEvent += LoadContentEvent;

            screen.ScreenManager.AddScreen(splash,
                screen.ControllingPlayer);

            MusicVolume = 1f;
        }

        #endregion

        #region Clear Level

        /// <summary>
        /// Clears all bullets from the level.
        /// </summary>
        private void DestroyAllBullets()
        {
            for (int i = 0; i < enemyBullets.Count; i++)
            {
                if (enemyBullets[i].CollisionCircle.radius < enemyBullets[i].DamageRadius)
                {
                    explosion.AddParticles(enemyBullets[i].Position, enemyBullets[i].DamageRadius);
                    explosionSmoke.AddParticles(enemyBullets[i].Position, enemyBullets[i].DamageRadius);
                }

                bulletQueue.Enqueue(enemyBullets[i]);
            }

            enemyBullets.Clear();

            for (int i = 0; i < playerBullets.Count; i++)
            {
                if (playerBullets[i].CollisionCircle.radius < playerBullets[i].DamageRadius)
                {
                    explosion.AddParticles(playerBullets[i].Position, playerBullets[i].DamageRadius);
                    explosionSmoke.AddParticles(playerBullets[i].Position, playerBullets[i].DamageRadius);
                }

                bulletQueue.Enqueue(playerBullets[i]);
            }

            playerBullets.Clear();
        }

        /// <summary>
        /// Clears all enemies from the level.
        /// </summary>
        private void DestroyAllEnemies()
        {
            int i = Enemies.Count - 1;
            if (i >= 0)
            {
                do
                {
                    enemies[i].State = EnemyState.Dead;

                    if (enemies[i] is Tank)
                    {
                        Tank t = enemies[i] as Tank;
                        t.MoveCue.Stop(AudioStopOptions.Immediate);
                        screen.SFXManager.PlayCue("Tank Death");
                    }

                    if (enemies[i] is Heli)
                    {
                        Heli h = enemies[i] as Heli;
                        screen.SFXManager.PlayCue("Heli Death");
                    }

                    for (int j = 0; j < enemySpawnPoints.Count; j++)
                        enemySpawnPoints[j].isEnemyActive = false;

                    explosionSmoke.AddParticles(enemies[i].CollisionPolygon.Center,
                        enemies[i].CollisionPolygon.Radius * 1.5f);

                    explosion.AddParticles(enemies[i].CollisionPolygon.Center,
                        enemies[i].CollisionPolygon.Radius * 1.5f);

                    enemies.RemoveAt(i);
                    i--;
                }
                while (enemies.Count > 0);
            }

            tanksToKill = 0;
            helisToKill = 0;
        }

        /// <summary>
        /// Clears all destructible tiles from the level.
        /// </summary>
        private void DestroyAllTiles()
        {
            
        }

        #endregion

        #region Other

        /// <summary>
        /// Checks one bullet for collisions with all the tiles on the map.
        /// </summary>
        private void DoBulletCollisionWithTiles(BulletCollisionCircle circle)
        {
            Rectangle bounds = circle.circle.BoundingBox;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            leftTile = (int)MathHelper.Max(leftTile, 0);
            topTile = (int)MathHelper.Max(topTile, 0);
            rightTile = (int)MathHelper.Min(rightTile, Width - 1);
            bottomTile = (int)MathHelper.Min(bottomTile, tiles.GetLength(1) - 1);

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    if (GetCollision(x, y) != TileCollision.Passable &&
                        Collisions.DoCollision(GetTilePolygon(x, y), circle.circle))
                    {
                        if (tiles[x, y].Destructible)
                        {
                            tiles[x, y].TakeDamage(circle.damage);

                            if (tiles[x, y].Health <= 0)
                            {
                                tileExplosion.AddParticles(new Vector2(
                                        x * Tile.Width + Tile.Width / 2,
                                        y * Tile.Height + Tile.Height / 2),
                                    Tile.Height);

                                for (int a = Math.Max(x - 1, 0); a <= Math.Min(x + 1, tiles.GetLength(0) - 1); a++)
                                    for (int b = Math.Max(y - 1, 0); b <= Math.Min(y + 1, tiles.GetLength(1) - 1); b++)
                                        if (tiles[a, b].Collision == TileCollision.Impassable)
                                            LevelLoader.FindNonCollidableEdges(a, b);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }
        
        /// <summary>
        /// Event handler for when the game ends, initiating the final cutscene.
        /// </summary>
        void LoadFinalCutscene()
        {
            DestroyAllBullets();
            DestroyAllEnemies();
            DestroyAllTiles();
        }

        /// <summary>
        /// Loads a final splash screen when the game is over, clears the
        /// current save game data, and sends the player back to the main menu.
        /// </summary>
        void EndGame()
        {
            One_Man_Army_Game.CampaignGameData.MaxWave = 24;
            Game.SaveGameData(One_Man_Army.One_Man_Army_Game.CampaignGameData, One_Man_Army_Game.FileName_Game_Campaign);

            SplashScreen splash = new SplashScreen(false, true, 5);

            splash.BackgroundEvent += GameOverEvent;

            screen.ScreenManager.AddScreen(splash,
                screen.ControllingPlayer);
        }

        /// <summary>
        /// Event handler for when the splash screen reaches its intermediate state, 
        /// and content can be loaded.
        /// </summary>
        void LoadContentEvent(object sender, EventArgs e)
        {
            LoadContent();
        }

        /// <summary>
        /// Event triggered when the player wins the game. It simply returns him to the main menu.
        /// </summary>
        void GameOverEvent(object sender, EventArgs e)
        {
            Thread.Sleep(2000);
            LoadingScreen.Load(Screen.ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
        }

        #endregion

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Player == null)
                return;
            
            Rectangle viewport = new Rectangle(spriteBatch.GraphicsDevice.Viewport.X, spriteBatch.GraphicsDevice.Viewport.Y,
                spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);

            if (timeRemaining <= 0)
            {
                Game.GraphicsDevice.Clear(Color.Black);
                float c = 1 - (0 - timeRemaining) / 3;
                spriteBatch.Begin();
                spriteBatch.Draw(whiteTex, viewport, Color.White * c);
                spriteBatch.End();
                return;
            }

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);

            spriteBatch.Begin();
            layers[0].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            smokePlumes1.Draw(gameTime, spriteBatch, cameraPosition * 0.35f);

            spriteBatch.Begin();
            layers[1].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            smokePlumes2.Draw(gameTime, spriteBatch, cameraPosition * 0.65f);

            spriteBatch.Begin();
            layers[2].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, cameraTransform);

            DrawTiles(spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            foreach (Bullet bullet in playerBullets)
                bullet.Draw(spriteBatch, false);

            foreach (Bullet bullet in enemyBullets)
                bullet.Draw(spriteBatch, player.IsRageMode);

            foreach (PowerUp pwrup in powerUps)
                pwrup.Draw(spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            tileExplosion.Draw(gameTime, spriteBatch, cameraTransform);
            explosionSmoke.Draw(gameTime, spriteBatch, cameraTransform);
            explosion.Draw(gameTime, spriteBatch, cameraTransform);
            smokeTrails.Draw(gameTime, spriteBatch, cameraTransform);

            weaponText.Draw(gameTime, spriteBatch, cameraTransform);

            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles.
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);

            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Rectangle source = new Rectangle(
                            tiles[x, y].TileToDraw * (Tile.Width + 2) + 1, 1, Tile.Width, Tile.Height);
                        Vector2 position = new Vector2(x, y) * Tile.Size;

                        spriteBatch.Draw(texture, position, source, Color.White);

                        // If the tile is damaged enough, draw the damage overlay over it.
                        if (tiles[x, y].Destructible && tiles[x, y].Health < 0.5f)
                            spriteBatch.Draw(tiles[x, y].DamagedTexture, position, Color.White);
                    }
                }
            }
        }

        private void ScrollCamera(Viewport viewport)
        {
            float maxCameraPosition = Tile.Width * Width - viewport.Width;

            const float ViewMargin = 0.35f;

            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = marginWidth;
            float marginRight = Tile.Width * Width - marginWidth;

            float lerpFactor = player.Position.X / (Tile.Width * Width);
            lerpFactor /= (Tile.Width * Width - marginWidth * 2) / (Tile.Width * Width);
            lerpFactor -= marginWidth / (Tile.Width * Width - marginWidth * 2);

            // Calculate how far to scroll when the player is near the edges of the screen.
            if (Player.Position.X > marginLeft && Player.Position.X < marginRight)
                cameraPosition = MathHelper.Lerp(0f, maxCameraPosition, lerpFactor);
        }


        #endregion
    }
}
