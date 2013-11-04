using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace One_Man_Army
{
    /// <summary>
    /// Enum which stores the position of the origin.
    /// </summary>
    public enum OriginType
    {
        BottomMiddle,
        LeftMiddle,
        RightMiddle,
        Center
    }

    /// <summary>
    /// Controls playback of an Animation.
    /// </summary>
    public struct AnimationPlayer
    {
        /// <summary>
        /// Gets the animation which is currently playing.
        /// </summary>
        public Animation Animation
        {
            get { return animation; }
        }
        Animation animation;

        /// <summary>
        /// Gets the index of the current frame in the animation.
        /// </summary>
        public int FrameIndex
        {
            get { return frameIndex; }
        }
        int frameIndex;

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        Color color;

        /// <summary>
        /// The amount of time in seconds that the current frame has been shown for.
        /// </summary>
        private float time;

        public OriginType OriginType
        {
            get { return originType; }
            set { originType = value; }
        }
        OriginType originType;

        /// <summary>
        /// Gets a texture origin at the bottom center of each frame.
        /// </summary>
        public Vector2 Origin
        {
            get
            {
                switch (originType)
                {
                    case OriginType.BottomMiddle:
                        origin = new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight);
                        break;

                    case OriginType.LeftMiddle:
                        origin = new Vector2(0f, Animation.FrameHeight / 2.0f);
                        break;

                    case OriginType.RightMiddle:
                        origin = new Vector2(Animation.FrameWidth, Animation.FrameHeight / 2.0f);
                        break;

                    case OriginType.Center:
                        origin = new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight / 2.0f);
                        break;

                    default:
                        return Vector2.Zero;
                }

                return origin;
            }
        }
        Vector2 origin;

        /// <summary>
        /// Begins or continues playback of an animation.
        /// </summary>
        public void PlayAnimation(Animation animation)
        {
            // If this animation is already running, do not restart it.
            if (Animation == animation)
                return;

            // Start the new animation.
            this.animation = animation;
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        /// <summary>
        /// Restarts the current animation.
        /// </summary>
        public void Restart()
        {
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        /// <summary>
        /// Advances the time position.
        /// </summary>
        public void Update(float elapsed)
        {
            // Process passing time.
            time += elapsed;
            while (time > Animation.FrameTime)
            {
                time -= Animation.FrameTime;

                // Advance the frame index; looping or clamping as appropriate.
                if (Animation.IsLooping)
                {
                    frameIndex = (frameIndex + 1) % Animation.FrameCount;
                }
                else
                {
                    frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                }
            }

        }

        /// <summary>
        /// Draws the current frame of the animation.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, 
            SpriteEffects spriteEffects, float rotation)
        {
            if (Animation == null)
                throw new NotSupportedException("No animation is currently playing.");

            if (color.A == 0)
                color = Color.White;

            // Calculate the source rectangle of the current frame.
            Rectangle source = new Rectangle(FrameIndex * Animation.FrameWidth, 0,
                Animation.FrameWidth, Animation.Texture.Height);

            // Draw the current frame.
            spriteBatch.Draw(Animation.Texture, position, source, color, 
                rotation, Origin, 1.0f, spriteEffects, 0.0f);
        }
    }
}
