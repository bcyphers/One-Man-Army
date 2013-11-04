using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace One_Man_Army
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class HUD
    {
        #region Fields

        private SpriteFont hudFont;
        private SpriteFont bigFont;

        private Texture2D damageOverlay;
        private Texture2D rageBarEmpty;
        private Texture2D rageBarFull;
        private Texture2D whiteOverlay;

        private float overlayAlpha = 0;
        public float OverlayAlpha
        {
            get { return overlayAlpha; }
            set { overlayAlpha = value; }
        }

        #endregion

        public HUD(ContentManager content)
        {
            hudFont = content.Load<SpriteFont>("Fonts/Hud");
            bigFont = content.Load<SpriteFont>("Fonts/bigfont");
            damageOverlay = content.Load<Texture2D>("HUD/Damaged Overlay");
            rageBarEmpty = content.Load<Texture2D>("HUD/Rage Bar Empty");
            rageBarFull = content.Load<Texture2D>("HUD/Rage Bar Full");
            whiteOverlay = content.Load<Texture2D>("HUD/White Overlay");
        }

        /// <summary>
        /// Activates a white flash that is drawn over the level, but not over the HUD.
        /// </summary>
        public void AddHealth()
        {
            overlayAlpha = -1f;
        }

        #region Draw

        public void Draw(SpriteBatch spriteBatch, Level level, Viewport viewport)
        {
            if (level.Player == null)
                return;

            spriteBatch.Begin();

            DrawOverlays(spriteBatch, level, viewport);

            Rectangle titleSafeArea = viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            Color color;
            switch (level.CurrentTime)
            {
                case TimeOfDay.Night:
                    color = Color.LightGray;
                    break;
                default:
                    color = Color.Black;
                    break;
            }

            if (level.CurrentState == GameState.InGame)
            {
                if (overlayAlpha > 0)
                    color.R = (byte)(overlayAlpha * 255);
                DrawPlayerData(spriteBatch, level, viewport, color);
            }

            // Draw Supply Drop overlay text
            string supplyString = "Supply Drop!";
            hudLocation = new Vector2(titleSafeArea.Center.X - bigFont.MeasureString(supplyString).X / 2,
                titleSafeArea.Center.Y - bigFont.MeasureString(supplyString).Y * 2);
            if (level.CurrentState == GameState.InSupplyDrop)
                DrawShadowedString(spriteBatch, bigFont, supplyString, hudLocation, color);

            // Draw Wave overlay text
            string waveString = "Wave " + level.Wave.ToString();
            hudLocation = new Vector2(titleSafeArea.Center.X - bigFont.MeasureString(waveString).X / 2,
                titleSafeArea.Center.Y - bigFont.MeasureString(waveString).Y * 2);
            if (level.IsBetweenWaves)
                DrawShadowedString(spriteBatch, bigFont, waveString, hudLocation, color);

            spriteBatch.End();
        }

        private void DrawPlayerData(SpriteBatch spriteBatch, Level level, Viewport viewport, Color color)
        {
            Rectangle titleSafeArea = viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            int health = (int)Math.Round(level.Player.Health * 100, 0);
            string healthString = "HEALTH: " + health.ToString();
            float height = hudFont.MeasureString(healthString).Y * 1.2f;

            // Draw player health
            DrawShadowedString(spriteBatch, hudFont, healthString, hudLocation, color);
            hudLocation.Y += height;

            // Draw the player's weapon data
            string ammo = level.Player.CurrentWeapon.AmmoPerClip == 0 ? "Infinite" :
                level.Player.CurrentWeapon.Ammo.ToString() + "/" + (level.Player.CurrentWeapon.AmmoPerClip * 3).ToString();
            string wepName = level.Player.CurrentWeapon.Name;
            DrawShadowedString(spriteBatch, hudFont, wepName + ": " + ammo, hudLocation, color);
            hudLocation.Y += height;

            // Draw the RAGE meter
            DrawRageMeter(spriteBatch, level, viewport, color, hudLocation);

            // What wave are we on?
            string waveString = "Wave " + level.Wave.ToString();
            hudLocation = new Vector2(titleSafeArea.Right - hudFont.MeasureString(waveString).X,
                titleSafeArea.Y);
            DrawShadowedString(spriteBatch, hudFont, waveString, hudLocation, color);
            hudLocation.Y += height;

            string tanksString = "Tanks Left: " + (level.TanksToKill < int.MaxValue ? level.TanksToKill.ToString() : "-");
            string helisString = "Helis Left: " + (level.HelisToKill < int.MaxValue ? level.HelisToKill.ToString() : "-");

            // How many Tanks are remaining
            hudLocation.X = titleSafeArea.Right - hudFont.MeasureString(tanksString).X;
            DrawShadowedString(spriteBatch, hudFont, tanksString, hudLocation, color);
            hudLocation.Y += height;

            // How many Helis are remaining
            hudLocation.X = titleSafeArea.Right - hudFont.MeasureString(helisString).X;
            DrawShadowedString(spriteBatch, hudFont, helisString, hudLocation, color);
            hudLocation.Y += height;

            // If it is the final wave, draw how much time remains in the game. 
            if (level.Wave == 25 && level.CurrentState != GameState.InCutscene)
            {
                string timeString = "Air Strike Inbound in: 00";
                hudLocation.X = titleSafeArea.Center.X - hudFont.MeasureString(timeString).X / 2f;
                hudLocation.Y = titleSafeArea.Y;

                timeString = "Air Strike Inbound in: " + (int)level.TimeRemaining;
                DrawShadowedString(spriteBatch, hudFont, timeString, hudLocation, color);
                hudLocation.Y += height;
            }
        }

        private void DrawRageMeter(SpriteBatch spriteBatch, Level level, Viewport viewport, Color color, Vector2 position)
        {
            // Draw the Rage Mode HUD monitor
            int rageBarWidth = 128;
            int rageBarHeight = 64;

            Rectangle rageSourceRect = new Rectangle(0, 0, (int)(rageBarFull.Width * level.Player.Rage),
                    (int)rageBarFull.Height);

            Rectangle rageDestRect1 = new Rectangle((int)position.X, (int)position.Y,
                    (int)(rageBarWidth * level.Player.Rage), (int)rageBarHeight);
            Rectangle rageDestRect2 = new Rectangle((int)position.X, (int)position.Y,
                    (int)rageBarWidth, (int)rageBarHeight);

            Color color1 = Color.Red;
            Color color2 = color;

            if (level.Player.IsRageMode)
            {
                rageDestRect1 = new Rectangle((int)(viewport.Width / 2 - rageBarWidth),
                    (int)position.Y / 2, (int)(rageBarWidth * level.Player.Rage) * 2,
                    (int)rageBarHeight * 2);
                rageDestRect2 = new Rectangle((int)(viewport.Width / 2 - rageBarWidth),
                    (int)position.Y / 2, (int)rageBarWidth * 2, (int)rageBarHeight * 2);

                color1 *= 0.5f;
            }

            if (level.Player.Rage >= 0.01)
                spriteBatch.Draw(rageBarFull, rageDestRect1, rageSourceRect, color1);

            spriteBatch.Draw(rageBarEmpty, rageDestRect2, color2);

            position.Y += rageDestRect2.Height * 1.2f;

            if (level.Player.Rage > 0.999f)
            {
#if !XBOX
                DrawShadowedString(spriteBatch, hudFont, "Press R to Activate!", position, color);
#else
                DrawShadowedString(spriteBatch, hudFont, "Press X to Activate!", position, color);
#endif
            }

        }

        /// <summary>
        /// Draw the white or gray overlays on top of the regular level elements.
        /// </summary>
        private void DrawOverlays(SpriteBatch spriteBatch, Level level, Viewport viewport)
        {
            if (overlayAlpha < 0)
            {
                overlayAlpha += .025f;

                spriteBatch.Draw(whiteOverlay, new Rectangle(0, 0, viewport.Width, viewport.Height),
                    Color.White * (Math.Abs(overlayAlpha % 1)));
            }
            else
            {
                if (level.Player.Health <= level.Player.MaxHealth)
                    overlayAlpha = 1 - level.Player.Health / level.Player.MaxHealth;
                else
                    overlayAlpha -= .005f;

                overlayAlpha = MathHelper.Max(overlayAlpha, (1 - level.Player.MaxHealth) * .6f);

                spriteBatch.Draw(damageOverlay, new Rectangle(0, 0, viewport.Width, viewport.Height),
                        Color.White * overlayAlpha);
            }
        }

        /// <summary>
        /// Draws two strings, with one black and slightly behind the other.
        /// </summary>
        private void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font,
            string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }

        #endregion
    }
}