#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using EasyStorage;
#endregion

namespace One_Man_Army
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry musicVolumeMenuEntry;
        MenuEntry sfxVolumeMenuEntry;
        MenuEntry resetDataMenuEntry;

        public static int MusicVolume = 100;
        public static int SFXVolume = 100;
        public static bool Is3D = false;

        #endregion

        #region Initialization
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options")
        {
            // Create our menu entries.
            musicVolumeMenuEntry = new MenuEntry(string.Empty);
            sfxVolumeMenuEntry = new MenuEntry(string.Empty);
            resetDataMenuEntry = new MenuEntry("Reset Game Data");
            
            if (One_Man_Army_Game.SaveDevice.FileExists(One_Man_Army_Game.ContainerName, One_Man_Army_Game.FileName_Options))
            {
                One_Man_Army_Game.SaveDevice.Load(
                    One_Man_Army_Game.ContainerName,
                    One_Man_Army_Game.FileName_Options,
                    stream =>
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            MusicVolume = int.Parse(reader.ReadLine());
                            SFXVolume = int.Parse(reader.ReadLine());
                        }
                    }
                );
            }

            SetMenuEntryText();

            MenuEntry backMenuEntry = new MenuEntry("Back");

            backMenuEntry.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(musicVolumeMenuEntry);
            MenuEntries.Add(sfxVolumeMenuEntry);
            MenuEntries.Add(resetDataMenuEntry);
            MenuEntries.Add(backMenuEntry);
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            musicVolumeMenuEntry.Text = "Music Volume: " + MusicVolume + "%";
            sfxVolumeMenuEntry.Text = "Sound Effect Volume: " + SFXVolume + "%";
        }


        #endregion

        #region Handle Input

        public override void HandleInput(InputState input)
        {
            PlayerIndex index;

            if (input.IsMenuSelect(ControllingPlayer, out index) && selectedEntry == 2)
            {
                Game.SFXBank.PlayCue("Menu Select");

                string message = "Clear all data?";

                MessageBoxScreen prompt = new MessageBoxScreen(message);

                prompt.Accepted += ClearSaveGameDataAccepted;
                prompt.Cancelled += ClearSaveGameDataCancelled;

                ScreenManager.AddScreen(prompt, ControllingPlayer);
            }

            if (input.IsMenuLeft(ControllingPlayer))
            {
                Game.SFXBank.PlayCue("Menu LeftRight");
                
                if (selectedEntry == 0)
                    MusicVolumeMenuEntryDown();

                if (selectedEntry == 1)
                    SFXVolumeMenuEntryDown();
            }

            if (input.IsMenuRight(ControllingPlayer))
            {
                Game.SFXBank.PlayCue("Menu LeftRight");

                if (selectedEntry == 0)
                    MusicVolumeMenuEntryUp();

                if (selectedEntry == 1)
                    SFXVolumeMenuEntryUp();
            }

            base.HandleInput(input);
        }

        /// <summary>
        /// Override OnCancel so that options can be saved when the player backs out of the screen.
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            // Make sure the device is ready.
            if (One_Man_Army_Game.SaveDevice.IsReady)
            {
                // save a file asynchronously. this will trigger IsBusy to return true
                // for the duration of the save process.
                One_Man_Army_Game.SaveDevice.SaveAsync(
                    One_Man_Army_Game.ContainerName,
                    One_Man_Army_Game.FileName_Options,
                    stream =>
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.WriteLine(MusicVolume);
                            writer.WriteLine(SFXVolume);
                        }
                    }
                );
            }

            base.OnCancel(playerIndex);
        }

        /// <summary>
        /// Music volume is increased.
        /// </summary>
        void MusicVolumeMenuEntryUp()
        {
            MusicVolume += 10;

            if (MusicVolume > 100)
                MusicVolume = 100;

            SetMusicVolume();
            SetMenuEntryText();
        }

        /// <summary>
        /// Music volume is decreased.
        /// </summary>
        void MusicVolumeMenuEntryDown()
        {
            MusicVolume -= 10;

            if (MusicVolume < 0)
                MusicVolume = 0;

            SetMusicVolume();
            SetMenuEntryText();
        }

        /// <summary>
        /// Sets the volume of the music to the current value.
        /// </summary>
        void SetMusicVolume()
        {
            AudioCategory category = ((One_Man_Army_Game)ScreenManager.Game).AudioEngine.GetCategory("Music");

            category.SetVolume((float)MusicVolume / 200);
        }

        /// <summary>
        /// SFX volume is increased.
        /// </summary>
        void SFXVolumeMenuEntryUp()
        {
            SFXVolume += 10;
            
            if (SFXVolume > 100)
                SFXVolume = 100;

            SetSFXVolume();
            SetMenuEntryText();
        }

        /// <summary>
        /// SFX volume is decreased.
        /// </summary>
        void SFXVolumeMenuEntryDown()
        {
            SFXVolume -= 10;

            if (SFXVolume < 0)
                SFXVolume = 0;

            SetSFXVolume();
            SetMenuEntryText();
        }


        /// <summary>
        /// Sets the SFX volume to the current value.
        /// </summary>
        void SetSFXVolume()
        {
            AudioCategory category = ((One_Man_Army_Game)ScreenManager.Game).AudioEngine.GetCategory("Default");

            category.SetVolume((float)SFXVolume / 100);
        }

        /// <summary>
        /// Event handler for when the player elects to clear his saved hame data.
        /// </summary>
        void ClearSaveGameDataAccepted(object sender, PlayerIndexEventArgs e)
        {
            One_Man_Army_Game.CampaignGameData.Clear();
            Game.SaveGameData(One_Man_Army_Game.CampaignGameData, One_Man_Army_Game.FileName_Game_Campaign);
            Game.SaveGameData(One_Man_Army_Game.CampaignGameData, One_Man_Army_Game.FileName_Game_Survival);

            resetDataMenuEntry.Text = "Data Cleared";
        }

        /// <summary>
        /// Event handler for when the user selects cancel on the "clear data" message box.
        /// </summary>
        void ClearSaveGameDataCancelled(object sender, PlayerIndexEventArgs e)
        {
        }

        #endregion
    }
}
