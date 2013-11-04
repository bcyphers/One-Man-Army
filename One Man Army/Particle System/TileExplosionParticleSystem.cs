using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace One_Man_Army
{
    public class TileExplosionParticleSystem : ParticleSystem
    {
        public TileExplosionParticleSystem(Game game, int howManyEffects)
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

            minInitialSpeed = 10;
            maxInitialSpeed = 40;

            // doesn't matter what these values are set to, acceleration is tweaked in
            // the override of InitializeParticle.
            minAcceleration = 0;
            maxAcceleration = 0;

            // explosions should be relatively short lived
            minLifetime = 1f;
            maxLifetime = 1.5f;

            // All particles should be almost the same size
            minScale = 1.2f;
            maxScale = 1.5f;

            minScaleAddend = 0;
            maxScaleAddend = 0;

            minNumParticles = 20;
            maxNumParticles = 30;

            minRotationSpeed = -MathHelper.PiOver2;
            maxRotationSpeed = MathHelper.PiOver2;

            blendState = BlendState.AlphaBlend;
        }

        /// <summary>
        /// InitializeParticle is overridden to add the appearance of gravity.
        /// </summary>
        /// <param name="p">the particle to set up</param>
        /// <param name="where">where the particle should be placed</param>
        protected override void InitializeParticle(ExplosionParticle p, Vector2 where, float scalar)
        {
            base.InitializeParticle(p, where, 0.3f);

            // Instead of all in one place, these particles will be randomly dispersed within a rectangle.
            float relSpeedX = p.Velocity.X * 4 / maxInitialSpeed;
            float relSpeedY = p.Velocity.Y * 4 / maxInitialSpeed;

            p.Position += new Vector2(24 * relSpeedX, 16 * relSpeedY);
        }
    }
}
