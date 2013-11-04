#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace One_Man_Army
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        MenuEntry playGameMenuEntry;
        MenuEntry survivalModeMenuEntry;
        MenuEntry optionsMenuEntry;
        MenuEntry statsMenuEntry;
        MenuEntry controlsMenuEntry;
        MenuEntry exitMenuEntry;

        #region Initialization
        
        /// <summary>z
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("One Man Army")
        {
            // Create our menu entries.
            playGameMenuEntry = new MenuEntry("Start Game");
            survivalModeMenuEntry = new MenuEntry("Survival Mode");
            optionsMenuEntry = new MenuEntry("Options");
            statsMenuEntry = new MenuEntry("Stats");
            controlsMenuEntry = new MenuEntry("Controls");
            exitMenuEntry = new MenuEntry("Exit");
            
            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            survivalModeMenuEntry.Selected += SurvivalModeMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            statsMenuEntry.Selected += StatsMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(playGameMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(statsMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

        public override void LoadContent()
        {
            if (!ScreenManager.MenuMusicCue.IsPlaying)
            {
                ScreenManager.MenuMusicCue = Game.MusicBank.GetCue("Menu Music");
                ScreenManager.MenuMusicCue.Play();
            }

            base.LoadContent();
        }

        public override void UnloadContent()
        {
            ScreenManager.MenuMusicCue.Stop(AudioStopOptions.Immediate);
            base.UnloadContent();
        }

        #endregion

        #region Handle Input

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (One_Man_Army_Game.CampaignGameData.IsCleared)
                playGameMenuEntry.Text = "Start Game";
            else
                playGameMenuEntry.Text = "Continue Game";

            if (One_Man_Army_Game.IsCampaignFinished)
            {
                if (!MenuEntries.Contains(survivalModeMenuEntry))
                    MenuEntries.Insert(1, survivalModeMenuEntry);
            }
            else
            {
                if (MenuEntries.Contains(survivalModeMenuEntry))
                    MenuEntries.Remove(survivalModeMenuEntry);
            }
                
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            Game.SFXBank.PlayCue("Menu Select"); 
            
            GameplayScreen screen = new GameplayScreen(0);
            screen.StartingWave = One_Man_Army_Game.CampaignGameData.CurrentWave;
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, screen,
                               new SplashScreen(One_Man_Army_Game.CampaignGameData.CurrentWave % 6 == 0,
                                   true, (One_Man_Army_Game.CampaignGameData.CurrentWave / 6)));
        }
        
        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void SurvivalModeMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            Game.SFXBank.PlayCue("Menu Select");
            ScreenManager.AddScreen(new SurvivalMenuScreen(), ControllingPlayer);
        }

        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            Game.SFXBank.PlayCue("Menu Select");
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }


        /// <summary>
        /// Event handler for when the Stats meny entry is selected.
        /// </summary>
        void StatsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            Game.SFXBank.PlayCue("Menu Select");
            ScreenManager.AddScreen(new StatsScreen(), e.PlayerIndex);
        }

        /// <summary>
        /// Event handler for when the player attempts to buy the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BuyMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            Game.GetGamerTag(e.PlayerIndex);

            SignedInGamer gamer = Game.Gamer;

            if (gamer != null && gamer.IsSignedInToLive && gamer.Privileges.AllowPurchaseContent)
                Guide.ShowMarketplace(e.PlayerIndex);
            else
                ScreenManager.AddScreen(new MessageBoxScreen("Your account does not allow Marketplace purchases."),
                    e.PlayerIndex);
        }

        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            Game.SFXBank.PlayCue("Menu Back");
            string message = "Really quit?";

            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
            confirmExitMessageBox.Cancelled += ConfirmExitMessageBoxCancelled;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }

        /// <summary>
        /// Event handler for when the user selects cancel on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxCancelled(object sender, PlayerIndexEventArgs e)
        {
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        #endregion
    }
}
