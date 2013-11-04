using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace One_Man_Army
{
    public class TextParticleSystem : ParticleSystem
    {
        public TextParticleSystem(Game game, int howManyEffects)
            : base(game, howManyEffects)
        {
        }
        
        /// <summary>
        /// Set up the constants that will give this particle system its behavior and
        /// properties.
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = null;

            // long lifetime, this can be changed to create thinner or thicker smoke.
            // tweak minNumParticles and maxNumParticles to complement the effect.
            minLifetime = 0.8f;
            maxLifetime = 0.8f;

            minNumParticles = 1;
            maxNumParticles = 1;

            minScale = 0.6f;
            maxScale = 0.6f;

            minInitialSpeed = 10;
            maxInitialSpeed = 20;

            minRotationSpeed = -0.15f;
            maxRotationSpeed = 0.15f;

            blendState = BlendState.AlphaBlend;
        }

        /// <summary>
        /// Draws all particles in the system to the screen.
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch SpriteBatch, Matrix transform)
        {

            // tell sprite batch to begin, using the BlendState specified in
            // initializeConstants
            SpriteBatch.Begin(SpriteSortMode.Immediate, blendState, null, null, null, null, transform);

            var orderdParticles = from p in particles orderby -p.TimeSinceStart select p;

            foreach (ExplosionParticle p in orderdParticles)
            {
                // skip inactive particles
                if (!p.Active)
                    continue;

                float normalizedLifetime = (p.TimeSinceStart / p.Lifetime);

                normalizedLifetime = (float)Math.Asin(normalizedLifetime);

                Color color = Color.White * (1 - normalizedLifetime);

                // make particles grow as they age. they'll start at 75% of their size,
                // and increase to 100% once they're finished.
                float scale = p.Scale * (1f + .25f * (1 - normalizedLifetime));

                SpriteBatch.Draw(p.Texture, p.Position, null, color,
                    p.Rotation, origin, scale, SpriteEffects.None, 0.0f);
            }

            SpriteBatch.End();
        }

        /// <summary>
        /// AddParticles is overloaded to account for the differences in the 
        /// TextParticleSystem, namely, a unique texture for each weapon.
        /// </summary>
        public void AddParticles(Vector2 where, float scale, Texture2D tex)
        {
            scale = (float)Math.Pow(scale, .8);

            if (tex == null)
                throw new InvalidOperationException(
                    "The particle must be initialized with a non-null texture.");

            // create a particle, if you can.
            if (freeParticles.Count > 0)
            {
                // grab a particle from the freeParticles queue, and Initialize it.
                ExplosionParticle p = freeParticles.Dequeue();
                InitializeParticle(p, where, scale, tex);
            }
        }

        /// <summary>
        /// InitializeParticle is overloaded to add the texture variable.
        /// </summary>
        /// <param name="p">the particle to set up</param>
        /// <param name="where">where the particle should be placed</param>
        protected void InitializeParticle(ExplosionParticle p, Vector2 where, float scalar, Texture2D tex)
        {
            where.Y -= 40 * scalar;

            float lifetime = One_Man_Army_Game.RandomBetween(minLifetime, maxLifetime) / 2 +
                One_Man_Army_Game.RandomBetween(minLifetime, maxLifetime) / 2 * scalar;
            float scale = minScale * scalar;
            float rotation = One_Man_Army_Game.RandomBetween(minRotationSpeed, maxRotationSpeed);

            Vector2 velocity = new Vector2(0, -One_Man_Army_Game.RandomBetween(minInitialSpeed, maxInitialSpeed));
            Vector2 acceleration = new Vector2(0, -One_Man_Army_Game.RandomBetween(10, 20));

            velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(rotation));
            acceleration = Vector2.Transform(acceleration, Matrix.CreateRotationZ(rotation));

            // then initialize it with those random values. initialize will save those,
            // and make sure it is marked as active.
            p.Initialize(where, velocity, Vector2.Zero, lifetime, scale, 0, 0, tex);
            p.Rotation = rotation;
        }
    }
}
