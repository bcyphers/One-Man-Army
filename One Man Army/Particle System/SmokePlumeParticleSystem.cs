#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace One_Man_Army
{
    public struct SmokePlumeInstance
    {
        public SmokePlumeParticleSystem ParticleSystem;
        public Vector2 Position;
        public float Radius;
        float timeBetweenParticleAdditions;
        float timeSinceLastAddition;
        Level level;

        public SmokePlumeInstance(SmokePlumeParticleSystem system, Vector2 pos, float rad, Level level)
        {
            ParticleSystem = system;
            Position = pos;
            Radius = rad;
            timeBetweenParticleAdditions = 0.2f;
            timeSinceLastAddition = 0f;
            this.level = level;
        }

        public void Update(float elapsed)
        {
            timeSinceLastAddition += elapsed;

            if (timeSinceLastAddition >= timeBetweenParticleAdditions)
            {
                timeSinceLastAddition -= timeBetweenParticleAdditions;
                AddParticles();
            }
        }

        private void AddParticles()
        {
            ParticleSystem.AddParticles(Position, Radius);
        }
    }

    /// <summary>
    /// SmokePlumeParticleSystem is a specialization of ParticleSystem which sends up a
    /// plume of smoke. The smoke is blown to the right by the wind.
    /// </summary>
    public class SmokePlumeParticleSystem : ParticleSystem
    {
        Color color;

        public SmokePlumeParticleSystem(One_Man_Army_Game game, int howManyEffects, Color color)
            : base(game, howManyEffects)
        {
            this.color = color;
        }

        /// <summary>
        /// Set up the constants that will give this particle system its behavior and
        /// properties.
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "gray smoke";

            minInitialSpeed = 80;
            maxInitialSpeed = 100;

            // we don't want the particles to accelerate at all, aside from what we
            // do in our overriden InitializeParticle.
            minAcceleration = 0;
            maxAcceleration = 0;

            // long lifetime, this can be changed to create thinner or thicker smoke.
            // tweak minNumParticles and maxNumParticles to complement the effect.
            minLifetime = 5.0f;
            maxLifetime = 7.0f;

            minScale = .5f;
            maxScale = 1.0f;

            minNumParticles = 1;
            maxNumParticles = 1;

            // rotate slowly, we want a fairly relaxed effect
            minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
            maxRotationSpeed = MathHelper.PiOver4 / 2.0f;

            blendState = BlendState.AlphaBlend;
        }

        /// <summary>
        /// PickRandomDirection is overriden so that we can make the particles always 
        /// move have an initial velocity pointing up.
        /// </summary>
        /// <returns>a random direction which points basically up.</returns>
        protected override Vector2 PickRandomDirection()
        {
            // Point the particles somewhere between 80 and 100 degrees.
            // tweak this to make the smoke have more or less spread.
            float radians = One_Man_Army_Game.RandomBetween(
                MathHelper.ToRadians(80), MathHelper.ToRadians(100));

            Vector2 direction = Vector2.Zero;
            // from the unit circle, cosine is the x coordinate and sine is the
            // y coordinate. We're negating y because on the screen increasing y moves
            // down the monitor.
            direction.X = (float)Math.Cos(radians);
            direction.Y = -(float)Math.Sin(radians);
            return direction;
        }

        /// <summary>
        /// InitializeParticle is overridden to add the appearance of wind.
        /// </summary>
        /// <param name="p">the particle to set up</param>
        /// <param name="where">where the particle should be placed</param>
        protected override void InitializeParticle(ExplosionParticle p, Vector2 where, float scalar)
        {
            base.InitializeParticle(p, where, scalar);
        }

        public void Draw(GameTime gameTime, SpriteBatch SpriteBatch, float cameraPosition)
        {
            Matrix transform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);

            // tell sprite batch to begin, using the spriteBlendMode specified in
            // initializeConstants
            SpriteBatch.Begin(SpriteSortMode.Immediate, blendState, null, null, null, null, transform);

            foreach (ExplosionParticle p in particles)
            {
                // skip inactive particles
                if (!p.Active)
                    continue;

                // normalized lifetime is a value from 0 to 1 and represents how far
                // a particle is through its life. 0 means it just started, .5 is half
                // way through, and 1.0 means it's just about to be finished.
                // this value will be used to calculate alpha and scale, to avoid 
                // having particles suddenly appear or disappear.
                float normalizedLifetime = (p.TimeSinceStart - p.Lifetime / 2) / (p.Lifetime / 2);

                float alpha = (1 - normalizedLifetime);
                Color newColor = color * alpha;

                SpriteBatch.Draw(texture, p.Position, null, newColor,
                    p.Rotation, origin, p.Scale, SpriteEffects.None, 0.0f);
            }

            SpriteBatch.End();
        }
    }
}
