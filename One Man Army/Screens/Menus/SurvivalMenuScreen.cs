#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace One_Man_Army
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class SurvivalMenuScreen : MenuScreen
    {
        #region Initialization

        MenuEntry startingTimeEntry;
        int startingTime = 0;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public SurvivalMenuScreen()
            : base("Survival Mode")
        {
            // Create our menu entries.
            startingTimeEntry = new MenuEntry("");
            MenuEntry level1Entry = new MenuEntry("Training");
            MenuEntry level2Entry = new MenuEntry("Pitfall");
            MenuEntry level3Entry = new MenuEntry("Giza");
            MenuEntry level4Entry = new MenuEntry("Floating City");
            MenuEntry exitMenuEntry = new MenuEntry("Back");
            SetStartingWaveText();

            // Hook up menu event handlers.
            level1Entry.Selected += ContinueSelected;
            level2Entry.Selected += ContinueSelected;
            level3Entry.Selected += ContinueSelected;
            level4Entry.Selected += ContinueSelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startingTimeEntry);
            MenuEntries.Add(level1Entry);
            MenuEntries.Add(level2Entry);
            MenuEntries.Add(level3Entry);
            MenuEntries.Add(level4Entry);
            MenuEntries.Add(exitMenuEntry);
        }

        private void SetStartingWaveText()
        {
            startingTimeEntry.Text = "Starting Time: " + (TimeOfDay)startingTime;
        }

        #endregion

        #region Handle Input

        public override void HandleInput(InputState input)
        {
            if (selectedEntry == 0)
            {
                if (input.IsMenuLeft(ControllingPlayer))
                {
                    startingTime = (int)MathHelper.Max(startingTime - 1, 0);
                    Game.SFXBank.PlayCue("Menu LeftRight");
                }

                if (input.IsMenuRight(ControllingPlayer))
                {
                    startingTime = (int)MathHelper.Min(startingTime + 1, 3);
                    Game.SFXBank.PlayCue("Menu LeftRight");
                }

                SetStartingWaveText();
            }

            base.HandleInput(input);
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void ContinueSelected(object sender, PlayerIndexEventArgs e)
        {
            Game.SFXBank.PlayCue("Menu Select");

            int index = selectedEntry;
            GameplayScreen screen = new GameplayScreen(index);
            screen.StartingWave = startingTime * 6;

            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, screen,
                               new SplashScreen(false, true, startingTime));
        }
        
        #endregion
    }
}
