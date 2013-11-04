using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace One_Man_Army
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SFXManager : Microsoft.Xna.Framework.GameComponent
    {
		SoundBank bank;    
		string name;
        List<Cue> cueList;
		
        public SFXManager(Game game, SoundBank bank)
            : base(game)
        {
			this.bank = bank;  
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            cueList = new List<Cue>();
            base.Initialize();
        }

        /// <summary>
        /// Play a cue from the soundbank.
        /// </summary>
        /// <param name="name"></param>
        public void PlayCue(string name)
        {
            Cue cue = bank.GetCue(name);
            cue.Play();
            cueList.Add(cue);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < cueList.Count; i++ )
            {
                if (!cueList[i].IsPlaying && !cueList[i].IsPreparing)
                {
                    cueList[i].Dispose();
                    cueList.RemoveAt(i);
                    i--;
                }
            }

            base.Update(gameTime);
        }
    }
}
