using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace One_Man_Army
{
    class TransmissionScreen : GameScreen
    {
        #region Messages

        #region Dawn

        readonly string[] DAWN_MESSAGE = new string[] {

            //"Hi MIT!\n\n" +
            //"This is a little game I've been making, on and off, for\n" +
            //"about two years. As of 12/31/11, it's selling on the xbox\n" +
            //"live indie games marketplace. If anyone there has a 360,\n" +
            //"you can download the full game with this single-use code:\n\n" +
            //"C4Q2X-7Y84Q-CMR3J-KJK6C-K9M3W\n\n" +
            //"I hope you like it. I've definitely enjoyed making it. And\n" +
            //"I'd like nothing better than to spend the next 4 years\n" +
            //"making more from Boston.\n\n" +
//#if XBOX
//            "Press A to continue",
//#else
//            "Spacebar to continue",
//#endif

            "How To Play:\n\n" +
#if XBOX
            "Left analog stick or D-pad to move\n" +
            "A button or left trigger to jump\n" +
            "left and right bumpers to switch weapons\n" +
            "X to activate Rage (when bar is full)\n" +
            "Y to stab tanks, when their health is low\n" +
            "Start to pause\n\n" +
            "Press A to continue",
#else
            "WASD or arrow keys to move and jump\n" +
            "Q and E to switch weapons\n" +
            "R to activate Rage (when bar is full)\n" +
            "F to stab tanks, when their health is low\n" +
            "Escape to pause\n\n" +
            "Spacebar to continue",
#endif

            "The situation:\n\n" +
            "You are Army, retired Green Beret, active\n" +
            "badass. All is right with the world. You live alone in\n" +
            "a small, quiet town, and spend your free time wrestling\n" +
            "bears, punching cinder blocks, and eating nails. Then,\n" +
            "one fine, sunny morning, the peace is shattered: an army\n" +
            "of mysterious, malevolent drones attacks the U.S. in huge\n" +
            "numbers. The screams are deafening. And you wanted to\n" +
            "sleep in today.\n\n" +
#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif
            
            "The army is cut off. The air force can drop off a few\n" +
            "supplies and weapons. No one can spare any troops yet.\n" +
            "You have to hold your position until they can. You are\n" +
            "the city's only hope.\n\n" +
#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif
            
            //"Army... this is Hale, your old supervisor.\n" +
            //"Listen, I know you're retired, but there\n" +
            //"are special circumstances. We've just\n" +
            //"received word of what looks like the\n" +
            //"beginning of a large-scale assault, like\n" +
            //"nothing we've seen since the fourties.\n" +
            //"We don't know who, or what, the enemy is,\n" +
            //"but they've hit New York, Washington,\n" +
            //"Miami, LA... the list goes on. \n\n" +
            //"As we speak, a mechanized army of tanks\n" +
            //"and helicopter drones is closing in on\n" +
            //"your position.",

            //"We've mobilized the national guard and\n" +
            //"every troop in the country, but the nearest\n" +
            //"unit is still approximately 100 klicks away\n" +
            //"from your position, across rough terrain \n" +
            //"and heavy resistance. We can make periodic\n" +
            //"supply drops to the city, but until further\n" +
            //"notice, you're on your own.\n\n" +
            //"Army, we need you. Right now, you're the\n" +
            //"only man we've got.",

            "To do:\n" +
            "   Defend the city\n" +
            "   Survive\n\n" +
            "Guns:\n" +
            "   12 Guage Pump-Action Shotgun\n" +
            "   40mm Pump-Acton Grenade Launcher\n" +
            "   AK-47 Assault Rifle\n" +
            "   L.A.W. Rocket Launcher\n\n" +
#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif

        };

        #endregion
        
        #region Midday
        
        readonly string[] MIDDAY_MESSAGE = new string[] {
            
            //"EMERGENCY BROADCAST SERVICE: This is not\n" +
            //"a test. Foreign attack underway. All\n" +
            //"civilians advised to evacuate immediately.\n" +
            //"Main highways blocked; use of secondary\n" +
            //"routes recommended. If unable to evacuate,\n" +
            //"find shelter immediately. Military personnel\n" +
            //"will arrive shortly to assist in emergency\n" +
            //"management procedures. Marshall law is in\n" +
            //"effect. Stay away from exposed metropolitan\n" +
            //"areas. Repeat, this is not a test.\n",

            "The attack is underway in several major cities,\n" +
            "and the army is finally responding. All civillians\n" +
            "have been evacuated. Your city is cut off from the\n" +
            "rest of the country by an enemy battalion, but there\n" +
            "is a plan to break through. Backup should arrive in\n" +
            "12 hours.\n\n" +
#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif

            "To do:\n" +
            "   Minimize damage to city\n" +
            "   Survive\n\n" +
            "Guns:\n" +
            "   10 Guage Double Barrel Shotgun\n" +
            "   M32 MGL 40mm Grenade Launcher\n" +
            "   XM214 5.56mm Chaingun\n" +
            "   Stinger Guided Missile Launcher\n\n" +
#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif

        };
        
        #endregion

        #region Dusk
        
        readonly string[] DUSK_MESSAGE = new string[] {
            
            //"Army priority radio: All troops and\n" +
            //"national guard forces are to report to\n" +
            //"the nearest base immediately, and regroup\n" +
            //"for the counter-attack. Some info on the\n" +
            //"enemy's strength, and if anyone is still\n" +
            //"the metro area, fall back immediately.",

            "The enemy is much stornger than anyone thought.\n" +
            "There are massive losses across the board. All\n" +
            "forces are regrouping in a few major hubs to\n" +
            "plan the counter-attack. Your city will be one of\n" +
            "the first to be liberated. Just hold out until dawn.\n\n" +
#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif

            "To do:\n" +
            "   Hold position and wait for backup\n" +
            "   Survive\n\n" +
            "Guns:\n" +
            "   SPAS 12 Semi-Automatic Shotgun\n" +
            "   M-29 Davy Crockett Tactical Nuke\n" +
            "   Enhanced XM214 w/ Incindiery Rounds\n" +
            "   3X Guided Missile \"Shrocket\" Launcher\n\n" +
#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif

        };

        #endregion

        #region Night
        
        readonly string[] NIGHT_MESSAGE = new string[] {
            
            //"Desperate S.O.S. from other troops in the\n" +
            //"city, saying they have failed, and are\n" +
            //"pinned down. Transmission cuts off, implying\n" +
            //"they are killed.",

            "Radio contact is sporadic, and HQ hasn't been\n" +
            "responding. The counter-attack was less successful\n" +
            "than anticipated, but the plan is still on. Stay\n" +
            "alive until morning. It's going to be a long night.\n\n" +
#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif

            "Current objectives:\n" +
            "   Survive\n\n\n" +
            "Ordinance:\n" +
            "   SPAS 12 Semi-Automatic Shotgun\n" +
            "   M-29 Davy Crockett Tactical Nuke\n" +
            "   Enhanced XM214 w/ Incindiery Rounds\n" +
            "   3X Guided Missile \"Shrocket\" Launcher\n\n" +

#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif

        };

        #endregion

        #region Dawn2

        readonly string[] DAWN2_MESSAGE = new string[] {

            "The army has given up, and is going to nuke\n" +
            "the city to control the threat. The airstrike\n" +
            "arrives in 60 seconds. Kill everything you can.\n\n" +
#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif

            "To do:\n" +
            "   Kill Everything\n\n" +
            "Guns:\n" +
            "   All of them\n\n" +
#if XBOX
            "Press A to continue",
#else
            "Spacebar to continue",
#endif

        };

        #endregion

        #endregion

        List<string> message = new List<string>();
        string toDraw = "";
        int stringIndex = 0;
        float timer = 0;
        int pageNumber = 0;
        TimeOfDay timeOfDay;
        bool shouldScroll = false;

        public TransmissionScreen(TimeOfDay time)
        {
            timeOfDay = time;

            switch (time)
            {
                case TimeOfDay.Dawn:
                    for (int i = 0; i < DAWN_MESSAGE.Length; i++)
                        this.message.Add(DAWN_MESSAGE[i]);
                    break;
                case TimeOfDay.Midday:
                    for (int i = 0; i < MIDDAY_MESSAGE.Length; i++)
                        this.message.Add(MIDDAY_MESSAGE[i]);
                    break;
                case TimeOfDay.Dusk:
                    for (int i = 0; i < DUSK_MESSAGE.Length; i++)
                        this.message.Add(DUSK_MESSAGE[i]);
                    break;
                case TimeOfDay.Night:
                    for (int i = 0; i < NIGHT_MESSAGE.Length; i++)
                        this.message.Add(NIGHT_MESSAGE[i]);
                    break;
                case TimeOfDay.Dawn2:
                    for (int i = 0; i < DAWN2_MESSAGE.Length; i++)
                        this.message.Add(DAWN2_MESSAGE[i]);
                    break;
            }
            
            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0);
        }

        /// <summary>
        /// Update the screen, periodically adding new text.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            char[] charArray = message[pageNumber].ToCharArray();

            shouldScroll = (timer >= 2f && stringIndex < charArray.Length - 1) || shouldScroll;

            if (shouldScroll)
                toDraw += charArray[stringIndex];

            shouldScroll = false;

            if (stringIndex < charArray.Length - 1 && timer >= 2f)
            {
                stringIndex += 1;
                shouldScroll = true;
            }
        }

        /// <summary>
        /// Handles input, looking for an A-button press to advance to the next screen of text.
        /// </summary>
        /// <param name="input"></param>
        public override void HandleInput(InputState input)
        {
            PlayerIndex index;
            if (input.IsNewButtonPress(Buttons.A, ControllingPlayer, out index) ||
                input.IsNewKeyPress(Keys.Space, ControllingPlayer, out index))
                NextPage();

            base.HandleInput(input);
        }

        /// <summary>
        /// Advances to the next page of text, if there is a next page to advance to.
        /// </summary>
        private void NextPage()
        {
            if (stringIndex < message[pageNumber].ToCharArray().Length - 1)
            {
                toDraw = message[pageNumber];
                stringIndex = message[pageNumber].ToCharArray().Length - 1;
                shouldScroll = false;
                timer = 2f;
                return;
            }

            if (pageNumber < message.Count - 1)
            {
                toDraw = "";
                stringIndex = 0;
                pageNumber++;
                shouldScroll = true;
            }
            else
            {
                ExitScreen();
            }
        }

        /// <summary>
        /// Draw the splash screen. The screen will fade in, display the splash 
        /// while loading is done in the background, and then fade out.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            Vector2 viewportSize = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height);

            ScreenManager.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            Color color = Color.White;
            SpriteFont font = ScreenManager.BigFont;
            float scale = 1f;
            string titleString = "INCOMING TRANSMISSION";
            Vector2 textSize = font.MeasureString(titleString);
            Vector2 textPosition = new Vector2(viewportSize.X - textSize.X * scale,
                ScreenManager.GraphicsDevice.Viewport.TitleSafeArea.Y + 50) / 2;

            if ((int)timer == (int)(timer + 0.5f) || timer >= 2f)
                spriteBatch.DrawString(font, titleString, textPosition, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

            font = ScreenManager.Font;
            scale = 1f;
            textSize = font.MeasureString(message[pageNumber]);
            textPosition = (viewportSize - textSize * scale) / 2;

            spriteBatch.DrawString(font, toDraw, textPosition, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

            spriteBatch.End();
        }
    }
}
