using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace One_Man_Army
{
    /// <summary>
    /// Stores the data for an enemy spawn point, including the type of enemy,
    /// the location, and whether or not an enemy has spawned there.
    /// </summary>
    public class EnemySpawnPoint
    {
        public Vector2 location;
        public string spriteSet;
        public bool isEnemyActive;
        public Type enemyType;

        public EnemySpawnPoint(Vector2 loc, string set, Type type)
        {
            location = loc;
            spriteSet = set;
            enemyType = type;
            isEnemyActive = false;
        }
    }

    /// Facing direction along the X axis.
    /// </summary>
    public enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }

    public enum EnemyState
    {
        Spawning,
        Alive,
        Dead
    }

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    public class Enemy
    {
        public Level Level
        {
            get { return level; }
        }
        protected Level level;

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }
        protected Vector2 position;

        /// <summary>
        /// The true center of the enemy, for homing bullets.
        /// </summary>
        public virtual Vector2 Center
        {
            get { return position; }
        }

        /// <summary>
        /// Health of the enemy
        /// </summary>
        public float Health
        {
            get { return health; }
            set { health = value; }
        }
        protected float health;

        /// <summary>
        /// The enemy's health when it is full
        /// </summary>
        public virtual float TotalHealth
        {
            get { return 1; }
        }

        /// <summary>
        /// A tracker used to deal damage to the enemy; it also creates a one-frame
        /// delay for hit registration that allows the enemy to flash red.
        /// </summary>
        public float DamageToTake
        {
            get { return damageToTake; }
        }
        protected float damageToTake;

        /// <summary>
        /// The current state of the enemy (dead, alive, spawning).
        /// </summary>
        public EnemyState State
        {
            get { return state; }
            set { state = value; }
        }
        protected EnemyState state;

        /// <summary>
        /// Used for collisions with bullets and the player.
        /// </summary>
        public Polygon CollisionPolygon
        {
            get { return polygon; }
        }
        protected Polygon polygon;

        /// <summary>
        /// The enemy's identification number, describing which type of enemy it is.
        /// </summary>
        protected int id;
        public int ID
        {
            get { return id; }
        }

        /// <summary>
        /// The point where the enemy spawned.
        /// </summary>
        public EnemySpawnPoint SpawnPoint;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        protected FaceDirection direction = FaceDirection.Left;

        protected float spawnTime = 0;
        protected const float MaxSpawnTime = 1f;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, int ident)
        {
            this.level = level;
            this.id = ident;
        }

        /// <summary>
        /// Spawns an enemy at the specified position, and with the specified sprite set and spawn point.
        /// </summary>
        public virtual void InitEnemy(Vector2 position, EnemySpawnPoint point)
        {
            this.position = position;
            this.SpawnPoint = point;
            this.state = EnemyState.Spawning;
            this.spawnTime = 0f;
        }

        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public virtual void Update(float elapsed)
        {
            health -= damageToTake;
            damageToTake = 0;

            if (health <= 0)
                state = EnemyState.Dead;

            if (state == EnemyState.Spawning)
            {
                spawnTime += elapsed;
                if (spawnTime >= MaxSpawnTime)
                    state = EnemyState.Alive;
            }
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
        }

        public void TakeDamage(float amount) 
        {
            damageToTake += amount;
        }

        public Enemy Clone()
        {
            return this.MemberwiseClone() as Enemy;
        }
    }
}
