#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace One_Man_Army
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class PurchaseMenuScreen : MenuScreen
    {
        #region Initialization

        string message = "You have reached the end of this limited trial.\n\r"
            + "Buy the full version to finish the campaign,\n\r"
            + "track your stats, and play unlimited Survival mode.\n\r";

        /// <summary>
        /// Constructor.
        /// </summary>
        public PurchaseMenuScreen()
            : base("Buy Full Game")
        {
            // Flag that there is no need for the game to transition
            // off when the pause menu is on top of it.
            IsPopup = true;

            // Create our menu entries.
            MenuEntry messageMenuEntry = new MenuEntry(message);
            MenuEntry buyGameMenuEntry = new MenuEntry("Buy Full Version");
            MenuEntry quitGameMenuEntry = new MenuEntry("Return to Menu");
            
            // Hook up menu event handlers.
            buyGameMenuEntry.Selected += BuyGameMenuEntrySelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(messageMenuEntry);
            MenuEntries.Add(buyGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }


        #endregion

        #region Handle Input

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            if (selectedEntry == 0)
                selectedEntry = 1;
        }

        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void BuyGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            Game.SFXBank.PlayCue("Menu Select");
            if (ControllingPlayer.HasValue)
                Game.AttemptBuyFullVersion(ControllingPlayer.Value);
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
        }

        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            Game.SFXBank.PlayCue("Menu Select");
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
        }

        #endregion

        #region Draw


        /// <summary>
        /// Draws the pause menu screen. This darkens down the gameplay screen
        /// that is underneath us, and then chains to the base MenuScreen.Draw.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            base.Draw(gameTime);
        }


        #endregion
    }
}
