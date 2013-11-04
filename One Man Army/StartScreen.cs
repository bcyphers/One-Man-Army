using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;


namespace One_Man_Army
{
    class StartScreen : GameScreen
    {
        string startString = "Press Start";
        string titleString = "Trippin' Alien";

        public StartScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            if (!screenManager.MenuMusicCue.IsPlaying)
                screenManager.MenuMusicCue.Play();
        }

        public override void HandleInput(InputState input)
        {
            PlayerIndex index;

            if (input.IsMenuSelect(null, out index))
            {
                Game.SFXBank.PlayCue("Menu_MenuSelection");

                Game.InitializeGame(index);

                ScreenManager.AddScreen(new MainMenuScreen(), index);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            Vector2 position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, 
                ScreenManager.GraphicsDevice.Viewport.Height * 0.8f);
            Vector2 origin = font.MeasureString(startString) / 2;
            Color color = Color.White * (TransitionAlpha / 255f);

            Vector2 titlePosition = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2,
                ScreenManager.GraphicsDevice.Viewport.Height * 0.2f);
            Vector2 titleOrigin = font.MeasureString(titleString) / 2;
            Color titleColor = Color.Yellow * (TransitionAlpha / 255f);

            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
                position.X -= transitionOffset * 256;
            else
                position.X += transitionOffset * 512;

            titlePosition.Y -= transitionOffset * 150;

            spriteBatch.Begin();

            spriteBatch.DrawString(font, startString, position, color, 0,
                origin, 1, SpriteEffects.None, 0);

            spriteBatch.DrawString(font, titleString, titlePosition, titleColor, 0,
                titleOrigin, 1.5f, SpriteEffects.None, 0);

            spriteBatch.End();
        }
    }
}