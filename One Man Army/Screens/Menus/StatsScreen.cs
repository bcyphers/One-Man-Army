using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace One_Man_Army
{
    class StatsScreen : MenuScreen
    {
        bool isCampaign = true;

        // Create our menu entries.
        MenuEntry CurrentInstanceMenuEntry;
        MenuEntry[] entries;

        public StatsScreen()
            : base("Statistics")
        {
            CurrentInstanceMenuEntry = new MenuEntry("");
            MenuEntries.Add(CurrentInstanceMenuEntry);

            // Create our menu entries.
            entries = new MenuEntry[SaveGameData.NUM_DATA];
            for (int i = 0; i < SaveGameData.NUM_DATA; i++)
                entries[i] = new MenuEntry("");
            
            // Add entries to the menu.
            for (int i = 0; i < SaveGameData.NUM_DATA; i++)
                MenuEntries.Add(entries[i]);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            SetMenuEntryText();
        }

        /// <summary>
        /// Sets the text of the entries, loading data based on the currentInstance 
        /// variable from one of the player's saved games.
        /// </summary>
        void SetMenuEntryText()
        {
            SaveGameData data;

            data = isCampaign ? One_Man_Army_Game.CampaignGameData : 
                Game.LoadGameData(One_Man_Army_Game.FileName_Game_Survival);

            CurrentInstanceMenuEntry.Text = isCampaign ? "Campaign Mode" : "Survival Mode";

            entries[0].Text = "Highest Wave Achieved: " + (data.MaxWave + 1).ToString();
            entries[1].Text = "Time Served: " +
                data.TimePlayed.Hours.ToString() + " Hours, " +
                data.TimePlayed.Minutes.ToString() + " Minutes, " +
                data.TimePlayed.Seconds.ToString() + " Seconds";
            entries[2].Text = "Ultimate Sacrifices: " + data.Deaths.ToString();
            entries[3].Text = "Tanks Destroyed: " + data.TanksKilled.ToString();
            entries[4].Text = "Helis Downed: " + data.HelisKilled.ToString();
            entries[5].Text = "Pain Delivered: " + ((int)(data.DamageDealt * 100)).ToString();
            entries[6].Text = "Damage Taken: " + ((int)(data.DamageTaken * 100)).ToString();
        }

        /// <summary>
        /// Draws the list of data to the screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            Vector2 position = new Vector2(100, 150);

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
                position.X -= transitionOffset * 256;
            else
                position.X += transitionOffset * 512;

            spriteBatch.Begin();

            // Draw each menu entry in turn.
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                MenuEntry menuEntry = MenuEntries[i];

                menuEntry.Draw(this, position, selectedEntry == i, gameTime);

                if (selectedEntry == i)
                    position.Y += menuEntry.GetHeight(this);

                position.Y += menuEntry.GetHeight(this);
            }

            // Draw the menu title.
            Vector2 titlePosition = new Vector2(426, 80);
            Vector2 titleOrigin = font.MeasureString("Statistics") / 2;
            Color titleColor = new Color(192, 192, 192, TransitionAlpha);
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, "Statistics", titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }

        #region HandleInput

        /// <summary>
        /// Overrides the HandleInput method because this is not a normal menu: there are several
        /// entries, but only one may be selected and handle input.
        /// </summary>
        /// <param name="input"></param>
        public override void HandleInput(InputState input)
        {
            PlayerIndex playerIndex;

            if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
            {
                OnCancel(playerIndex);
            }

            if (input.IsMenuLeft(ControllingPlayer) || input.IsMenuRight(ControllingPlayer))
            {
                Game.SFXBank.PlayCue("Menu LeftRight");
                CurrentInstanceMenuEntrySwitch();
            }
        }

        /// <summary>
        /// The player selects a more recent saved game. 
        /// </summary>
        void CurrentInstanceMenuEntrySwitch()
        {
            if (One_Man_Army_Game.IsCampaignFinished)
                isCampaign = !isCampaign;
            else
                isCampaign = true;

            SetMenuEntryText();
        }
        
        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
        }

        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            Game.SFXBank.PlayCue("Menu Back");
            ExitScreen();
        }

        #endregion
    }
}
