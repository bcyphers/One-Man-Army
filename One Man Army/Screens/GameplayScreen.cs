#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Media;
using DataTypeLibrary;
#endregion

namespace One_Man_Army
{
    /// <summary>
    /// This screen implements the actual game logic.
    /// </summary>
    public class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteBatch spriteBatch;
        int levelDataIndex;

        // Extra viewports for 3D
        Rectangle leftViewport;
        Rectangle rightViewport;

        RenderTarget2D renderTargetL;
        RenderTarget2D renderTargetR;

        int startingWave = 0;
        public int StartingWave
        {
            get { return levelDataIndex == 0 ? One_Man_Army_Game.CampaignGameData.CurrentWave : 0; }
            set { startingWave = value; }
        }

        HUD hud;
        public HUD HUD
        {
            get { return hud; }
        }

        SFXManager sfxManager;
        public SFXManager SFXManager
        {
            get { return sfxManager; }
        }

        Cue musicCue;
        public Cue MusicCue
        {
            get { return musicCue; }
            set { musicCue = value; }
        }
        
        // Meta-level game state.
        private Level level;

        Random random = new Random();

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen(int index)
        {
            levelDataIndex = index;

            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            this.Game = (One_Man_Army_Game)this.ScreenManager.Game;
             /*
            leftViewport = new Rectangle(0, ScreenManager.GraphicsDevice.Viewport.Height / 4,
                ScreenManager.GraphicsDevice.Viewport.Width / 2,
                ScreenManager.GraphicsDevice.Viewport.Height / 2);
            rightViewport = new Rectangle(leftViewport.Width + 1, leftViewport.Y,
                leftViewport.Width, leftViewport.Height);

            renderTargetL = new RenderTarget2D(ScreenManager.GraphicsDevice,
                ScreenManager.GraphicsDevice.Viewport.Width, 
                ScreenManager.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            renderTargetR = new RenderTarget2D(ScreenManager.GraphicsDevice,
                ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            */

            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            // Load fonts
            hud = new HUD(content);
            sfxManager = new SFXManager(this.Game, this.Game.SFXBank);
            Game.Components.Add(sfxManager);

            LoadLevel(null);

            if (level.Player != null)
                level.Player.ControllingPlayer = ControllingPlayer.Value;

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            Game.Components.Remove(sfxManager);
            musicCue.Dispose();
            content.Unload();
        }


        #endregion

        #region Update

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive || (level.Player != null && !level.Player.IsAlive))
            {
                level.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                
                if (IsActive)
                {
                    if (musicCue.IsPaused)
                        musicCue.Resume();
                }
            }
            
            if (!IsActive)
            {
                if (musicCue.IsPlaying && !musicCue.IsPaused)
                    musicCue.Pause();
                if (level.Player.RageModeCue.IsPlaying)
                    level.Player.RageModeCue.Pause();
            }
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if ((input.IsPauseGame(ControllingPlayer) || gamePadDisconnected) 
                && level.CurrentState != GameState.InTransition && level.CurrentState != GameState.InCutscene)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                if (level.Player == null)
                    return;
                else if (!level.Player.IsAlive)
                {
                    MessageBoxScreen youDiedMessageBox = new MessageBoxScreen("Killed in Action\nA to continue", false);
                    youDiedMessageBox.Accepted += ReloadCurrentLevelEvent;
                    ScreenManager.AddScreen(youDiedMessageBox, this.ControllingPlayer);
                }
                else
                    level.Player.HandleInput(input);
            }
        }

        #endregion

        #region Loading

        /// <summary>
        /// Loads the next level in the sequence.
        /// </summary>.
        private void LoadLevel(Player player)
        {
            // Find the path of the next level.
            string levelPath;

            // The path to the level data file. It is stored as a color-coded bitmap.
            levelPath = "Levels/" + levelDataIndex;

            if (levelDataIndex == 0)
                startingWave = One_Man_Army_Game.CampaignGameData.CurrentWave;

            // Load the level.
            level = new Level(Game.Services, levelPath, Game, this, startingWave, levelDataIndex != 0);
            if (player != null)
                level.Player = player;

            hud.OverlayAlpha = 0;
        }

        /// <summary>
        /// Reloads the current level.
        /// </summary>
        private void ReloadCurrentLevel()
        {
            LevelLoader.Reset();
            LoadLevel(null);
            level.Player.ControllingPlayer = ControllingPlayer.Value;
            hud.OverlayAlpha = 0;
        }

        /// <summary>
        /// Event handler for this method.
        /// </summary>
        void ReloadCurrentLevelEvent(object sender, PlayerIndexEventArgs e)
        {
            ReloadCurrentLevel();
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.Black);
            spriteBatch = ScreenManager.SpriteBatch;
            ScreenManager.GraphicsDevice.Clear(Color.Black);

            //ScreenManager.GraphicsDevice.SetRenderTarget(renderTargetL);
            
            level.Draw(gameTime, spriteBatch);
            hud.Draw(spriteBatch, level, ScreenManager.GraphicsDevice.Viewport);

            //ScreenManager.GraphicsDevice.SetRenderTarget(null);
            /*
            ScreenManager.GraphicsDevice.SetRenderTarget(renderTargetR);

            level.Draw(gameTime, spriteBatch);
            hud.Draw(spriteBatch, level, ScreenManager.GraphicsDevice.Viewport);

            ScreenManager.GraphicsDevice.SetRenderTarget(null);
            ScreenManager.GraphicsDevice.Clear(Color.Black);
            
            spriteBatch.Begin();
            spriteBatch.Draw(renderTargetL, leftViewport, Color.White);
            spriteBatch.Draw(renderTargetR, rightViewport, Color.White);
            spriteBatch.End();
 **/

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }

        #endregion
    }
}
