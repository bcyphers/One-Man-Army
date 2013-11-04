using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace One_Man_Army
{
    public struct SmokeEmitterInstance
    {
        public SmokeEmitterParticleSystem ParticleSystem;
        public Vector2 Position;
        public float Radius;
        float timeBetweenParticleAdditions;
        float timeSinceLastAddition;

        public SmokeEmitterInstance(SmokeEmitterParticleSystem system, Vector2 pos, float rad)
        {
            ParticleSystem = system;
            Position = pos;
            Radius = rad;
            timeBetweenParticleAdditions = 0.05f;
            timeSinceLastAddition = 0f;
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
    /// SmokeEmitterParticleSystem is a specialization of ParticleSystem which creates smoky
    /// trails. It should be used through SmokeEmitterInstances to add particles, rather than
    /// adding them directly.
    /// </summary>
    public class SmokeEmitterParticleSystem : ParticleSystem
    {
        public SmokeEmitterParticleSystem(Game game, int howManyEffects)
            : base(game, howManyEffects)
        {
        }
        
        /// <summary>
        /// Set up the constants that will give this particle system its behavior and
        /// properties.
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "smoke";

            minInitialSpeed = 0;
            maxInitialSpeed = 0;

            // we don't want the particles to accelerate at all, aside from what we
            // do in our overriden InitializeParticle.
            minAcceleration = 0;
            maxAcceleration = 0;

            // long lifetime, this can be changed to create thinner or thicker smoke.
            // tweak minNumParticles and maxNumParticles to complement the effect.
            minLifetime = 2.0f;
            maxLifetime = 3.0f;

            minScale = 2f;
            maxScale = 2f;

            minScaleAddend = 0.1f * minScale;
            maxScaleAddend = 0.1f * maxScale;

            minNumParticles = 15;
            maxNumParticles = 15;

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
                MathHelper.ToRadians(-10), MathHelper.ToRadians(10));

            Vector2 direction = Vector2.Zero;
            // from the unit circle, cosine is the x coordinate and sine is the
            // y coordinate. We're negating y because on the screen increasing y moves
            // down the monitor.
            direction.X = (float)Math.Cos(radians);
            direction.Y = -(float)Math.Sin(radians);
            return direction;
        }

        public override void Draw(GameTime gameTime, SpriteBatch SpriteBatch, Matrix transform)
        {

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
                float normalizedLifetime = p.TimeSinceStart / p.Lifetime + p.Lifetime / 4;

                // we want particles to fade in and fade out, so we'll calculate alpha
                // to be (normalizedLifetime) * (1-normalizedLifetime). this way, when
                // normalizedLifetime is 0 or 1, alpha is 0. the maximum value is at
                // normalizedLifetime = .5, and is
                // (normalizedLifetime) * (1-normalizedLifetime)
                // (.5)                 * (1-.5)
                // .25
                // since we want the maximum alpha to be 1, not .25, we'll scale the 
                // entire equation by 4.
                float alpha = 3 * normalizedLifetime * (1 - normalizedLifetime);
                Color color = Color.White * alpha;

                SpriteBatch.Draw(texture, p.Position, null, color,
                    p.Rotation, origin, p.Scale, SpriteEffects.None, 0.0f);
            }

            SpriteBatch.End();
        }
    }
}
