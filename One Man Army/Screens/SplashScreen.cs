#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace One_Man_Army
{
    class SplashScreen : GameScreen
    {
        ContentManager content;
        string message;
        int time;
        float lifeTime = 0;
        const float TOTAL_LIFE = 2f;
        bool isTransmission;

        /// <summary>
        /// Event raised when the splash screen has finished transitioning on, 
        /// allowing data to load in the background.
        /// </summary>
        public event EventHandler BackgroundEvent;

        public SplashScreen(bool transmission, bool fadeIn, int time)
        {
            if (time <= 3)
                this.message = ((TimeOfDay)time).ToString();
            else if (time == 4)
                this.message = ((TimeOfDay)0).ToString();
            else
                this.message = "Game Over";

            this.time = time;
            this.isTransmission = transmission;
            TransitionOnTime = TimeSpan.FromSeconds(fadeIn ? 1 : 0);
            TransitionOffTime = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Update the screen. When finished transitioning on, load data in the background.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (this.ScreenState != ScreenState.TransitionOn &&
                this.ScreenState != ScreenState.TransitionOff && 
                !coveredByOtherScreen)
            {
                if (lifeTime == 0)
                {
                    AddTransmissionScreen();
                    //if (time <= 4)
                    //    Game.MusicBank.PlayCue(((TimeOfDay)time).ToString() + " Cue");
                }

                lifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (lifeTime >= TOTAL_LIFE)
                {
                    LoadBackgroundData();
                    this.ExitScreen();
                    ScreenManager.Game.ResetElapsedTime();
                }
            }
        }

        /// <summary>
        /// Draw the splash screen. The screen will fade in, display the splash 
        /// while loading is done in the background, and then fade out.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            ScreenManager.FadeBackBufferToBlack(TransitionAlpha);

            float alpha = ScreenState == ScreenState.Active ? 1 : 0;
            Vector2 viewportSize = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height);

            spriteBatch.Begin();

            Color color = Color.Red * alpha;
            float scale = 1.5f;
            SpriteFont font = ScreenManager.BigFont;
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = (viewportSize - textSize * scale) / 2;
            spriteBatch.DrawString(font, message, textPosition, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

            spriteBatch.End();
        }

        /// <summary>
        /// Method for raising the BackgroundEvent event.
        /// </summary>
        void LoadBackgroundData()
        {
            if (BackgroundEvent != null)
                BackgroundEvent(this, null);
        }

        /// <summary>
        /// Adds a transmission screen on top of the splash screen.
        /// </summary>
        void AddTransmissionScreen()
        {
            if (isTransmission)
            {
                TransmissionScreen transmission = new TransmissionScreen((TimeOfDay)time);
                ScreenManager.AddScreen(transmission, this.ControllingPlayer);
            }
        }
    }
}
