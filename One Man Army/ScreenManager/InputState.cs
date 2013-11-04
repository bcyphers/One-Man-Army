#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace One_Man_Army
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// methods for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        #region Fields

        public const int MaxInputs = 4;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;
        public MouseState CurrentMouseState;

        public readonly KeyboardState[] LastKeyboardStates;
        public readonly GamePadState[] LastGamePadStates;
        public MouseState LastMouseState;

        public readonly bool[] GamePadWasConnected;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];

            LastKeyboardStates = new KeyboardState[MaxInputs];
            LastGamePadStates = new GamePadState[MaxInputs];

            GamePadWasConnected = new bool[MaxInputs];
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            for (int i = 0; i < MaxInputs; i++)
            {
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
                LastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

                // Keep track of whether a gamepad has ever been
                // connected, so we can detect if it is unplugged.
                if (CurrentGamePadStates[i].IsConnected)
                {
                    GamePadWasConnected[i] = true;
                }
            }
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer,
                                            out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key) &&
                        LastKeyboardStates[i].IsKeyUp(key));
            }
            else
            {
                // Accept input from any player.
                return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
            }
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer,
                                                     out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].IsButtonDown(button) &&
                        LastGamePadStates[i].IsButtonUp(button));
            }
            else
            {
                // Accept input from any player.
                return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
            }
        }


        /// <summary>
        /// Checks for a "menu select" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) ||
                   IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
        }


        public bool IsNewThumbstickUp(PlayerIndex? controllingPlayer,
                                             out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].ThumbSticks.Left.Y > 0.5f &&
                        LastGamePadStates[i].ThumbSticks.Left.Y < 0.5f);
            }
            else
            {
                // Accept input from any player.
                return (IsNewThumbstickUp(PlayerIndex.One, out playerIndex) ||
                        IsNewThumbstickUp(PlayerIndex.Two, out playerIndex) ||
                        IsNewThumbstickUp(PlayerIndex.Three, out playerIndex) ||
                        IsNewThumbstickUp(PlayerIndex.Four, out playerIndex));
            }
        }


        public bool IsNewThumbstickDown(PlayerIndex? controllingPlayer,
                                             out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].ThumbSticks.Left.Y < -0.5f &&
                        LastGamePadStates[i].ThumbSticks.Left.Y > -0.5f);
            }
            else
            {
                // Accept input from any player.
                return (IsNewThumbstickDown(PlayerIndex.One, out playerIndex) ||
                        IsNewThumbstickDown(PlayerIndex.Two, out playerIndex) ||
                        IsNewThumbstickDown(PlayerIndex.Three, out playerIndex) ||
                        IsNewThumbstickDown(PlayerIndex.Four, out playerIndex));
            }
        }


        public bool IsNewThumbstickLeft(PlayerIndex? controllingPlayer,
                                             out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].ThumbSticks.Left.X < -0.5f &&
                        LastGamePadStates[i].ThumbSticks.Left.X > -0.5f);
            }
            else
            {
                // Accept input from any player.
                return (IsNewThumbstickLeft(PlayerIndex.One, out playerIndex) ||
                        IsNewThumbstickLeft(PlayerIndex.Two, out playerIndex) ||
                        IsNewThumbstickLeft(PlayerIndex.Three, out playerIndex) ||
                        IsNewThumbstickLeft(PlayerIndex.Four, out playerIndex));
            }
        }


        public bool IsNewThumbstickRight(PlayerIndex? controllingPlayer,
                                             out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].ThumbSticks.Left.X > 0.5f &&
                        LastGamePadStates[i].ThumbSticks.Left.X < 0.5f);
            }
            else
            {
                // Accept input from any player.
                return (IsNewThumbstickRight(PlayerIndex.One, out playerIndex) ||
                        IsNewThumbstickRight(PlayerIndex.Two, out playerIndex) ||
                        IsNewThumbstickRight(PlayerIndex.Three, out playerIndex) ||
                        IsNewThumbstickRight(PlayerIndex.Four, out playerIndex));
            }
        }

        /// <summary>
        /// Checks for a "menu up" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuUp(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
                   IsNewThumbstickUp(controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu down" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuDown(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
                   IsNewThumbstickDown(controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu up" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuLeft(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Left, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadLeft, controllingPlayer, out playerIndex) ||
                   IsNewThumbstickLeft(controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu down" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuRight(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Right, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadRight, controllingPlayer, out playerIndex) ||
                   IsNewThumbstickRight(controllingPlayer, out playerIndex);
        }

        /// <summary>
        /// Checks for a "pause the game" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsPauseGame(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }

        /// <summary>
        /// Returns true when the player is jumping.
        /// </summary>
        public bool IsPlayerJumping(PlayerIndex controllingPlayer)
        {
            int i = (int)controllingPlayer;
            
            // Check if the player wants to jump.
            return CurrentGamePadStates[i].IsButtonDown(Buttons.A) ||
                CurrentGamePadStates[i].Triggers.Left > 0.5f ||
                CurrentKeyboardStates[i].IsKeyDown(Keys.Space) ||
                CurrentKeyboardStates[i].IsKeyDown(Keys.Up) ||
                CurrentKeyboardStates[i].IsKeyDown(Keys.W);
        }

        /// <summary>
        /// Returns true if the jump button was newly pressed.
        /// </summary>
        public bool DidPlayerJump(PlayerIndex controllingPlayer)
        {
            int i = (int)controllingPlayer;

            // Check if the player wants to jump, and didn't last time.
            return (CurrentGamePadStates[i].IsButtonDown(Buttons.A)
                    && !LastGamePadStates[i].IsButtonDown(Buttons.A))
                || (CurrentGamePadStates[i].Triggers.Left > 0.5f
                    && LastGamePadStates[i].Triggers.Left <= 0.5f)
                || (CurrentKeyboardStates[i].IsKeyDown(Keys.Space)
                    && !LastKeyboardStates[i].IsKeyDown(Keys.Space))
                || (CurrentKeyboardStates[i].IsKeyDown(Keys.Up)
                    && !LastKeyboardStates[i].IsKeyDown(Keys.Up))
                || (CurrentKeyboardStates[i].IsKeyDown(Keys.W)
                    && !LastKeyboardStates[i].IsKeyDown(Keys.W));
        }

        /// <summary>
        /// Returns true if the player has newly pressed the crouch button (B or left shift).
        /// </summary>
        public bool IsToggleCrouch(PlayerIndex controllingPlayer)
        {
            return (IsNewButtonPress(Buttons.B, controllingPlayer, out controllingPlayer) ||
                IsNewKeyPress(Keys.LeftShift, controllingPlayer, out controllingPlayer));
        }

        /// <summary>
        /// Returns true if the player should fire his weapon.
        /// </summary>
        public bool IsPlayerFiring(PlayerIndex controllingPlayer)
        {
            int i = (int)controllingPlayer;

            return (CurrentGamePadStates[i].Triggers.Right > 0.25f) || 
                (CurrentMouseState.LeftButton == ButtonState.Pressed);
        }

        /// <summary>
        /// Returns true if the player is pressing the Finishing Move button (Y or F).
        /// </summary>
        /// <param name="controllingPlayer"></param>
        /// <returns></returns>
        public bool IsPlayerFinishingMove(PlayerIndex controllingPlayer)
        {
            return (IsNewButtonPress(Buttons.Y, controllingPlayer, out controllingPlayer) ||
                IsNewKeyPress(Keys.F, controllingPlayer, out controllingPlayer));
        }

        /// <summary>
        /// Returns a value, 1, 0, or -1, representing the player's weapon switch input.
        /// </summary>
        public int IsSwitchWeapon(PlayerIndex controllingPlayer)
        {
            int i = 0;

            if (IsNewButtonPress(Buttons.LeftShoulder, controllingPlayer, out controllingPlayer) ||
                IsNewKeyPress(Keys.Q, controllingPlayer, out controllingPlayer))
                i -= 1;
            if (IsNewButtonPress(Buttons.RightShoulder, controllingPlayer, out controllingPlayer) ||
                IsNewKeyPress(Keys.E, controllingPlayer, out controllingPlayer))
                i += 1;

            return i;
        }

        /// <summary>
        /// Returns whether the player wants to activate RAGE mode this frame.
        /// </summary>
        public bool IsActivateRageMode(PlayerIndex controllingPlayer)
        {
            return (IsNewButtonPress(Buttons.X, controllingPlayer, out controllingPlayer)) ||
                (IsNewKeyPress(Keys.R, controllingPlayer, out controllingPlayer));
        }

        /// <summary>
        /// Is the player trying to activate a finishing move?
        /// </summary>
        public bool IsActivateFinishingMove(PlayerIndex controllingPlayer)
        {
            return (CurrentGamePadStates[0].IsButtonDown(Buttons.Y)
                || CurrentKeyboardStates[0].IsKeyDown(Keys.F));
        }
        
        /// <summary>
        /// Gets the horizontal movement of the player, from -1 to 1.
        /// </summary>
        public float GetPlayerMovement(PlayerIndex controllingPlayer)
        {
            int i = (int)controllingPlayer;
            
            // Get analog horizontal movement.
            float movement = CurrentGamePadStates[i].ThumbSticks.Left.X;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // If any digital horizontal movement input is found, override the analog movement.
            if (CurrentGamePadStates[i].IsButtonDown(Buttons.DPadLeft) ||
                CurrentKeyboardStates[i].IsKeyDown(Keys.Left) ||
                CurrentKeyboardStates[i].IsKeyDown(Keys.A))
            {
                movement = -1.0f;
            }
            else if (CurrentGamePadStates[i].IsButtonDown(Buttons.DPadRight) ||
                     CurrentKeyboardStates[i].IsKeyDown(Keys.Right) ||
                     CurrentKeyboardStates[i].IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            return movement;
        }

        /// <summary>
        /// Returns the player's aim vector.
        /// </summary>
        public Vector2? GetPlayerAimDirection(PlayerIndex controllingPlayer)
        {
            int i = (int)controllingPlayer;

            Vector2 rightThumbStick = new Vector2(CurrentGamePadStates[i].ThumbSticks.Right.X,
                -CurrentGamePadStates[i].ThumbSticks.Right.Y);

            if (rightThumbStick.Length() >= 0.3f)
                return Vector2.Normalize(rightThumbStick);

            return null;
        }

        #endregion
    }
}
