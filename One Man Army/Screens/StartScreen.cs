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
    class StartScreen : MenuScreen
    {

        public StartScreen()
            : base("One Man Army")
        {
#if XBOX
            MenuEntry menuEntry = new MenuEntry("Press Start");
#else
            MenuEntry menuEntry = new MenuEntry("Press Spacebar");
#endif
            menuEntry.Selected += StartGame;
            MenuEntries.Add(menuEntry);
        }

        public override void LoadContent()
        {
            if (!ScreenManager.MenuMusicCue.IsPlaying)
                ScreenManager.MenuMusicCue.Play();

            base.LoadContent();
        }

        void StartGame(object sender, PlayerIndexEventArgs e)
        {
            Game.SFXBank.PlayCue("Menu Select");

            Game.InitializeGame(e.PlayerIndex);

            ScreenManager.AddScreen(new MainMenuScreen(), e.PlayerIndex);
        }

        /// <summary>
        /// When the user cancels, nothing will happen.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
        }
    }
}